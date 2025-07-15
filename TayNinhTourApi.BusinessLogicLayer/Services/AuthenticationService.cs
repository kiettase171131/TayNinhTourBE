using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Threading.Tasks;

using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.ForgotPasswordDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Authentication;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Authentication;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly BcryptUtility _bcryptUtility;
        private readonly JwtUtility _jwtUtility;
        private readonly IMemoryCache _memoryCache;
        private readonly EmailSender _emailSender;

        private readonly IHostingEnvironment _env;

        private readonly IUnitOfWork _unitOfWork;


        private readonly TimeSpan _otpExpiration = TimeSpan.FromMinutes(3);
        private readonly TimeSpan _lockoutDuration = TimeSpan.FromMinutes(5);




        public AuthenticationService(IHostingEnvironment env, IUserRepository userRepository, IMapper mapper, BcryptUtility bcryptUtility, JwtUtility jwtUtility, IRoleRepository roleRepository, IMemoryCache memoryCache, EmailSender emailSender,
            IUnitOfWork unitOfWork)

        {
            _userRepository = userRepository;
            _mapper = mapper;
            _bcryptUtility = bcryptUtility;
            _jwtUtility = jwtUtility;
            _roleRepository = roleRepository;
            _memoryCache = memoryCache;
            _emailSender = emailSender;

            _env = env;

            _unitOfWork = unitOfWork;

        }

        public async Task<BaseResposeDto> RegisterAsync(RequestRegisterDto request)
        {
            // Check if email already exists and has been verified
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null && existingUser.IsVerified)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Email is already registered and verified. Please login instead."
                };
            }

            // Generate OTP and save it to cache
            string otp = new Random().Next(100000, 999999).ToString();
            string otpKey = $"{request.Email}_{request.ClientIp}_OTP";
            _memoryCache.Set(otpKey, otp, _otpExpiration);

            // Send OTP to email
            await _emailSender.SendOtpRegisterAsync(request.Email, otp);

            User userToRegister;

            if (existingUser == null)
            {
                // Register new user
                userToRegister = _mapper.Map<User>(request);
                userToRegister.PasswordHash = _bcryptUtility.HashPassword(request.Password);
                userToRegister.IsVerified = false;
                userToRegister.IsActive = true;

                var role = await _roleRepository.GetRoleByNameAsync(Constants.RoleUserName);
                if (role == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 500,
                        Message = "Role not found"
                    };
                }

                userToRegister.RoleId = role.Id;
                userToRegister.Role = role;

                await _userRepository.AddAsync(userToRegister);
            }
            else
            {
                // Update unverified user (overwrite info + reset password)
                existingUser.Name = request.Name;
                existingUser.PasswordHash = _bcryptUtility.HashPassword(request.Password);
                existingUser.PhoneNumber = request.PhoneNumber;
                existingUser.Avatar = request.Avatar ?? existingUser.Avatar;
                existingUser.IsVerified = false;
                existingUser.IsActive = true;

                userToRegister = existingUser;
            }

            await _userRepository.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "OTP sent to your email. Please verify to complete registration.",
                success = true
            };
        }


        public async Task<ResponseAuthenticationDto> LoginAsync(RequestLoginDto request)
        {
            var includes = new string[]
            {
                nameof(User.Role)
            };

            // Find user by email
            var user = await _userRepository.GetUserByEmailAsync(request.Email, includes);
            // If user not exists, return error response
            if (user == null || !_bcryptUtility.VerifyPassword(request.Password, user.PasswordHash))
            {
                return new ResponseAuthenticationDto
                {
                    StatusCode = 400,
                    Message = "Invalid email or password!"
                };
            }
            // Check is user verified
            if (!user.IsVerified)
            {
                return new ResponseAuthenticationDto
                {
                    StatusCode = 400,
                    Message = "User is not verified, back to register to verify your email!"
                };
            }

            // Check is user locked
            if (!user.IsActive)
            {
                return new ResponseAuthenticationDto
                {
                    StatusCode = 400,
                    Message = "This account is not available at this time!"
                };
            }

            // Return success response
            return await GenerateTokenAsync(user);
        }

        private async Task<ResponseAuthenticationDto> GenerateTokenAsync(User newUser)
        {
            var token = _jwtUtility.GenerateToken(newUser);

            return new ResponseAuthenticationDto
            {
                Token = token,
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(newUser),
                TokenExpirationTime = DateTime.UtcNow.AddDays(Constants.TokenExpiredTime),
                UserId = newUser.Id,
                Avatar = newUser.Avatar,
                Email = newUser.Email,
                Name = newUser.Name,
                PhoneNumber = newUser.PhoneNumber,
                Role = newUser.Role.Name, // Add role to response
                StatusCode = 200,
                success = true
            };
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User newUser)
        {
            using (var transaction = _unitOfWork.BeginTransaction())
            {
                var refreshToken = _jwtUtility.GenerateRefreshToken();
                newUser.RefreshToken = refreshToken;
                newUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                return refreshToken;
            }
        }

        public async Task<ResponseVerifyOtpDto> VerifyOtpAsync(RegisterVerifyOtpRequestDto request)
        {
            if (string.IsNullOrEmpty(request.ClientIp))
            {
                return new ResponseVerifyOtpDto
                {
                    StatusCode = 400,
                    Message = "Can not verify user, try again!"
                };
            }

            // Find user by email
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            // If user not exists, return error response
            if (user == null)
            {
                return new ResponseVerifyOtpDto
                {
                    StatusCode = 400,
                    Message = "Email is not existed in the system"
                };
            }

            string failedAttemptsKey = $"FailedAttempts_IP_{request.ClientIp}";

            string lockoutKey = $"Lockout_IP_{request.ClientIp}";
            if (_memoryCache.TryGetValue(lockoutKey, out DateTime lockoutEnd) && lockoutEnd > DateTime.UtcNow)
            {
                var remainingTime = (lockoutEnd - DateTime.UtcNow).TotalMinutes;
                return new ResponseVerifyOtpDto
                {
                    StatusCode = 400,
                    Message = $"OTP is wrong, try again after {_lockoutDuration.TotalMinutes} minutes."
                };
            }

            string otpKey = $"{request.Email}_{request.ClientIp}_OTP";
            if (!_memoryCache.TryGetValue(otpKey, out string? storedOtp) || string.IsNullOrEmpty(storedOtp))
            {
                return new ResponseVerifyOtpDto
                {
                    StatusCode = 400,
                    Message = "OTP is not valid or expired."
                };
            }

            if (request.Otp == storedOtp)
            {
                _memoryCache.Remove(otpKey);
                _memoryCache.Remove(failedAttemptsKey);

                // Update user status to verified
                user.IsVerified = true;
                await _userRepository.SaveChangesAsync();

                return new ResponseVerifyOtpDto
                {
                    StatusCode = 200,
                    Message = "Your account is verified. You can login now!",
                    success = true
                };
            }
            else
            {
                // If OTP is incorrect, increment failed attempts
                int failedAttempts = _memoryCache.TryGetValue(failedAttemptsKey, out int attempts) ? attempts + 1 : 1;
                _memoryCache.Set(failedAttemptsKey, failedAttempts, _otpExpiration);

                if (failedAttempts >= Constants.MaxFailedAttempts)
                {
                    _memoryCache.Set(lockoutKey, DateTime.UtcNow.Add(_lockoutDuration), _lockoutDuration);
                    return new ResponseVerifyOtpDto
                    {
                        StatusCode = 400,
                        Message = $"OTP is wrong, try again after {_lockoutDuration.TotalMinutes} minutes."
                    };
                }
                else
                {
                    return new ResponseVerifyOtpDto
                    {
                        StatusCode = 400,
                        Message = $"OTP is wrong, you have {Constants.MaxFailedAttempts - failedAttempts} attempts left."
                    };
                }
            }
        }

        public async Task<ResponseAuthenticationDto> RefreshTokenAsync(RequestRefreshTokenDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user == null)
            {
                return new ResponseAuthenticationDto
                {
                    StatusCode = 400,
                    Message = "Refresh token không hợp lệ hoặc đã hết hạn."
                };
            }

            return await GenerateTokenAsync(user);
        }

        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await _unitOfWork.UserRepository!.FindUserByRefreshToken(userId, refreshToken);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }
            return user;
        }

        public async Task<BaseResposeDto> SendOTPResetPasswordAsync(SendOtpDTO request)
        {
            bool existingEmail = await _userRepository.CheckEmailExistAsync(request.Email);
            if (!existingEmail)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Email is not existed."
                };
            }
            // Generate OTP and save it to cache
            string otp = new Random().Next(100000, 999999).ToString();
            string otpKey = $"{request.Email}_{request.ClientIp}_OTP";
            _memoryCache.Set(otpKey, otp, _otpExpiration);

            // Send OTP to email
            await _emailSender.SendOtpResetPasswordAsync(request.Email, otp);

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "OTP sent to your email. Please verify to complete reset password.",
                success = true
            };
        }

        public async Task<ResponseVerifyOtpDto> ResetPassword(ResetPasswordDTO request)
        {
            if (string.IsNullOrEmpty(request.ClientIp))
            {
                return new ResponseVerifyOtpDto
                {
                    StatusCode = 400,
                    Message = "Can not verify user, try again!"
                };
            }

            // Find user by email
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            // If user not exists, return error response
            if (user == null)
            {
                return new ResponseVerifyOtpDto
                {
                    StatusCode = 400,
                    Message = "Email is not existed in the system"
                };
            }

            string failedAttemptsKey = $"FailedAttempts_IP_{request.ClientIp}";

            string lockoutKey = $"Lockout_IP_{request.ClientIp}";
            if (_memoryCache.TryGetValue(lockoutKey, out DateTime lockoutEnd) && lockoutEnd > DateTime.UtcNow)
            {
                var remainingTime = (lockoutEnd - DateTime.UtcNow).TotalMinutes;
                return new ResponseVerifyOtpDto
                {
                    StatusCode = 400,
                    Message = $"OTP is wrong, try again after {_lockoutDuration.TotalMinutes} minutes."
                };
            }

            string otpKey = $"{request.Email}_{request.ClientIp}_OTP";
            if (!_memoryCache.TryGetValue(otpKey, out string? storedOtp) || string.IsNullOrEmpty(storedOtp))
            {
                return new ResponseVerifyOtpDto
                {
                    StatusCode = 400,
                    Message = "OTP is not valid or expired."
                };
            }

            if (request.Otp == storedOtp)
            {
                _memoryCache.Remove(otpKey);
                _memoryCache.Remove(failedAttemptsKey);

                // Update user status to verified
                user.PasswordHash = _bcryptUtility.HashPassword(request.NewPassword);
                var a = await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
                if (a)
                    return new ResponseVerifyOtpDto
                    {
                        StatusCode = 200,
                        Message = "Reset Password Succcessfully !",
                        success = true
                    };
                else
                    return new ResponseVerifyOtpDto
                    {
                        StatusCode = 400,
                        Message = "Reset Password Failed !",
                    };
            }
            else
            {
                // If OTP is incorrect, increment failed attempts
                int failedAttempts = _memoryCache.TryGetValue(failedAttemptsKey, out int attempts) ? attempts + 1 : 1;
                _memoryCache.Set(failedAttemptsKey, failedAttempts, _otpExpiration);

                if (failedAttempts >= Constants.MaxFailedAttempts)
                {
                    _memoryCache.Set(lockoutKey, DateTime.UtcNow.Add(_lockoutDuration), _lockoutDuration);
                    return new ResponseVerifyOtpDto
                    {
                        StatusCode = 400,
                        Message = $"OTP is wrong, try again after {_lockoutDuration.TotalMinutes} minutes."
                    };
                }
                else
                {
                    return new ResponseVerifyOtpDto
                    {
                        StatusCode = 400,
                        Message = $"OTP is wrong, you have {Constants.MaxFailedAttempts - failedAttempts} attempts left."
                    };
                }
            }
        }
    }


}


