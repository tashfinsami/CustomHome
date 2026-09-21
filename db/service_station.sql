#CREATE DATABASE service_station;

USE service_station;

CREATE TABLE ServiceTokens
(
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TokenNumber INT NOT NULL,
    Status VARCHAR(20) NOT NULL,
    CreatedAt DATETIME NOT NULL
);