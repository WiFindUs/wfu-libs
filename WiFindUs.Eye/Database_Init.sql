﻿CREATE DATABASE IF NOT EXISTS wfu_eye_db;
USE wfu_eye_db;

SET foreign_key_checks = 0;
DROP TABLE IF EXISTS DeviceStates;
DROP TABLE IF EXISTS ArchivedWaypointResponders;
DROP TABLE IF EXISTS Devices;
DROP TABLE IF EXISTS NodeStates;
DROP TABLE IF EXISTS Waypoints;
DROP TABLE IF EXISTS Nodes;
DROP TABLE IF EXISTS Users;
SET foreign_key_checks = 1;

CREATE TABLE Users (
	ID 					BIGINT NOT NULL AUTO_INCREMENT,
	Created				DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	NameFirst			VARCHAR(32) NOT NULL,
	NameMiddle			VARCHAR(32) NOT NULL,
	NameLast			VARCHAR(32) NOT NULL,
	Type				VARCHAR(32) NOT NULL,
	PRIMARY KEY (ID)
);

CREATE TABLE Waypoints (
	ID					BIGINT NOT NULL AUTO_INCREMENT,
	Created				DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	Latitude 			DOUBLE NOT NULL,
	Longitude 			DOUBLE NOT NULL,
	Altitude 			DOUBLE NULL,
	Accuracy 			DOUBLE NULL,
	Type				VARCHAR(32) NOT NULL,
	Category			VARCHAR(32) NOT NULL,
	Description			VARCHAR(2048) NOT NULL,
	Severity			INT(9) NOT NULL DEFAULT 0,
	Code				INT(9) NOT NULL DEFAULT 0,
	Archived			TINYINT(1) NOT NULL DEFAULT 0, 
	ArchivedTime		DATETIME,
	NextWaypointID 		BIGINT NULL,
	ReportedByID 		BIGINT NULL,
	PRIMARY KEY (ID),
	CONSTRAINT `Reported by` FOREIGN KEY `Reported by` (ReportedByID) REFERENCES Users (ID) ON UPDATE CASCADE ON DELETE SET NULL,
	CONSTRAINT `Leads on to` FOREIGN KEY `Leads on to` (NextWaypointID) REFERENCES Waypoints (ID) ON UPDATE CASCADE ON DELETE SET NULL
);

CREATE TABLE Devices (
	ID					BIGINT NOT NULL,
	Created				DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	Type				VARCHAR(32) NOT NULL DEFAULT 'PHO',
	WaypointID			BIGINT NULL,
	PRIMARY KEY (ID),
	CONSTRAINT `Heading to waypoint` FOREIGN KEY `Heading to waypoint` (WaypointID) REFERENCES Waypoints (ID) ON UPDATE CASCADE ON DELETE SET NULL
);

CREATE TABLE DeviceStates (
	DeviceID			BIGINT NOT NULL,
	UserID				BIGINT NULL,
	Created				DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	Latitude 			DOUBLE NOT NULL,
	Longitude 			DOUBLE NOT NULL,
	Altitude 			DOUBLE NULL,
	Accuracy 			DOUBLE NULL,
	Humidity 			DOUBLE NULL,
	AirPressure 		DOUBLE NULL,
	Temperature 		DOUBLE NULL,
	LightLevel 			DOUBLE NULL,
	Charging 			TINYINT(1) NULL DEFAULT 0,
	BatteryLevel 		DOUBLE NULL,
	IPAddress			BIGINT NOT NULL DEFAULT 0,
	PRIMARY KEY (DeviceID, Created),
	CONSTRAINT `State of device` FOREIGN KEY `State of device` (DeviceID)	REFERENCES Devices (ID) ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `In use by user` FOREIGN KEY `In use by user` (UserID)		REFERENCES Users (ID) ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE TABLE ArchivedWaypointResponders (
	UserID     			BIGINT NOT NULL,
	WaypointID 			BIGINT NOT NULL,
	PRIMARY KEY (UserID, WaypointID),
	CONSTRAINT `Responded to by user` FOREIGN KEY `Responded to by user` (UserID)		REFERENCES Users (ID) ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `Responded to waypoint` FOREIGN KEY `Responded to waypoint` (WaypointID)	REFERENCES Waypoints (ID) ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE TABLE Nodes (
	ID 					BIGINT NOT NULL,
	Created 			DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY (ID)
);

CREATE TABLE NodeStates (
	NodeID 				BIGINT NOT NULL,
	Created 			DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	Voltage 			DOUBLE NULL,
	Latitude 			DOUBLE NOT NULL,
	Longitude 			DOUBLE NOT NULL,
	Altitude 			DOUBLE NULL,
	Accuracy 			DOUBLE NULL,
	IPAddress 			BIGINT NOT NULL DEFAULT 0,
	Number 				BIGINT NOT NULL DEFAULT 0,
	PRIMARY KEY (NodeID, Created),
	CONSTRAINT `Location of node` FOREIGN KEY `Location of node` (NodeID) REFERENCES Nodes (ID) ON UPDATE CASCADE ON DELETE CASCADE
);