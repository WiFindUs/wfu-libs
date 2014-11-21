CREATE DATABASE IF NOT EXISTS wfu_eye_db;
USE wfu_eye_db;

SET foreign_key_checks = 0;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS Waypoints;
DROP TABLE IF EXISTS Devices;
DROP TABLE IF EXISTS DeviceLogins;
DROP TABLE IF EXISTS DeviceLocations;
DROP TABLE IF EXISTS DeviceAtmospheres;
DROP TABLE IF EXISTS Nodes;
DROP TABLE IF EXISTS NodeLocations;
DROP TABLE IF EXISTS ArchivedWaypointResponders;
SET foreign_key_checks = 1;

CREATE TABLE Users (
	ID					int(9) UNSIGNED NOT NULL AUTO_INCREMENT,
	Created				datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	NameFirst			varchar(32) NOT NULL CHECK (LEN(NameFirst) > 0), 
	NameMiddle			varchar(32) NOT NULL CHECK (LEN(NameMiddle) > 0), 
	NameLast			varchar(32) NOT NULL CHECK (LEN(NameLast) > 0), 
	Type				varchar(32) NOT NULL,
	PRIMARY KEY (ID)
) ENGINE=InnoDB;

CREATE TABLE Waypoints (
	ID					int(9) UNSIGNED NOT NULL AUTO_INCREMENT,
	Created				datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	Latitude			DOUBLE NOT NULL CHECK (Latitude BETWEEN -90.0 AND 90.0), 
	Longitude			DOUBLE NOT NULL CHECK (Longitude BETWEEN -180.0 AND 180.0), 
	Altitude			DOUBLE, 
	Accuracy			DOUBLE CHECK (Accuracy IS NULL OR Accuracy BETWEEN 0.0 AND 9999.9999), 
	Type				varchar(32) NOT NULL,
	Category			varchar(32) NOT NULL,
	Description			varchar(2048) NOT NULL,
	Severity			int(9) NOT NULL DEFAULT 0,
	Code				int(9) NOT NULL DEFAULT 0,
	Archived			tinyint(1) NOT NULL DEFAULT 0, 
	ArchivedTime		datetime,
	ReportedByID		int(9) UNSIGNED,
	NextWaypointID		int(9) UNSIGNED,
	PRIMARY KEY (ID),
	CONSTRAINT `Reported by` FOREIGN KEY `Reported by` (ReportedByID) REFERENCES Users (ID) ON UPDATE CASCADE ON DELETE SET NULL,
	CONSTRAINT `Leads on to` FOREIGN KEY `Leads on to` (NextWaypointID) REFERENCES Waypoints (ID) ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE Devices (
	ID					int(9) UNSIGNED NOT NULL,
	Created				datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	Updated				datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	Type				varchar(32) NOT NULL DEFAULT 'PHO',
	IPAddress			int(9) UNSIGNED NOT NULL DEFAULT 0,
	WaypointID			int(9) UNSIGNED,
	PRIMARY KEY (ID),
	CONSTRAINT `Heading to waypoint` FOREIGN KEY `Heading to waypoint` (WaypointID) REFERENCES Waypoints (ID) ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE DeviceLogins (
	DeviceID			int(9) UNSIGNED NOT NULL,
	UserID				int(9) UNSIGNED,
	Created				datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY (DeviceID, Created),
	CONSTRAINT `Using device` FOREIGN KEY `Using device` (DeviceID)	REFERENCES Devices (ID) ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `In use by user` FOREIGN KEY `In use by user` (UserID)		REFERENCES Users (ID) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE DeviceLocations (
	DeviceID			int(9) UNSIGNED NOT NULL,
	Created				datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	Latitude			DOUBLE NOT NULL CHECK (Latitude BETWEEN -90.0 AND 90.0), 
	Longitude			DOUBLE NOT NULL CHECK (Longitude BETWEEN -180.0 AND 180.0), 
	Altitude			DOUBLE, 
	Accuracy			DOUBLE CHECK (Accuracy IS NULL OR Accuracy BETWEEN 0.0 AND 9999.9999), 
	PRIMARY KEY (DeviceID, Created),
	CONSTRAINT `Location of device` FOREIGN KEY `Location of device` (DeviceID) REFERENCES Devices (ID) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE DeviceAtmospheres (
	DeviceID			int(9) UNSIGNED NOT NULL,
	Created				datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	Humidity			DOUBLE, 
	AirPressure			DOUBLE, 
	Temperature			DOUBLE, 
	LightLevel			DOUBLE, 
	PRIMARY KEY (DeviceID, Created),
	CONSTRAINT `Atmosphere measured by device` FOREIGN KEY `Atmosphere measured by device` (DeviceID) REFERENCES Devices (ID) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE ArchivedWaypointResponders (
	UserID     int(9) UNSIGNED NOT NULL, 
	WaypointID int(9) UNSIGNED NOT NULL,
	PRIMARY KEY (UserID, WaypointID),
	CONSTRAINT `Responded to by user` FOREIGN KEY `Responded to by user` (UserID)		REFERENCES Users (ID) ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `Responded to waypoint` FOREIGN KEY `Responded to waypoint` (WaypointID)	REFERENCES Waypoints (ID) ON UPDATE CASCADE ON DELETE CASCADE
 ) ENGINE=InnoDB;

CREATE TABLE Nodes (
	ID					int(9) UNSIGNED NOT NULL,
	Created				datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	Updated				datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	IPAddress			int(9) UNSIGNED NOT NULL DEFAULT 0,
	Number				int(9) UNSIGNED NOT NULL DEFAULT 0,
	PRIMARY KEY (ID)
) ENGINE=InnoDB;

CREATE TABLE NodeLocations (
	NodeID				int(9) UNSIGNED NOT NULL,
	Created				datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	Latitude			DOUBLE NOT NULL CHECK (Latitude BETWEEN -90.0 AND 90.0), 
	Longitude			DOUBLE NOT NULL CHECK (Longitude BETWEEN -180.0 AND 180.0), 
	Altitude			DOUBLE, 
	Accuracy			DOUBLE CHECK (Accuracy IS NULL OR Accuracy BETWEEN 0.0 AND 9999.9999), 
	PRIMARY KEY (NodeID, Created),
	CONSTRAINT `Location of node` FOREIGN KEY `Location of node` (NodeID) REFERENCES Nodes (ID) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB;