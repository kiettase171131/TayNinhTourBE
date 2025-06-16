-- Drop and recreate myapp_db database
-- Execute this in MySQL as root user

-- Drop database if exists
DROP DATABASE IF EXISTS `myapp_db`;

-- Create database
CREATE DATABASE `myapp_db` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Use the database
USE `myapp_db`;

-- Verify database creation
SELECT 'myapp_db database recreated successfully' as Result;
