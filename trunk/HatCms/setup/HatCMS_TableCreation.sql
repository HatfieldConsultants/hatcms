CREATE TABLE  `contactdata` (
  `ContactId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `firstName` varchar(255) NOT NULL,
  `lastName` varchar(255) NOT NULL,
  `title` varchar(255) NOT NULL,
  `organizationName` varchar(255) NOT NULL,
  `address1` varchar(255) NOT NULL,
  `address2` varchar(255) NOT NULL,
  `city` varchar(255) NOT NULL,
  `provinceState` varchar(255) NOT NULL,
  `postalZipCode` varchar(255) NOT NULL,
  `phoneNumber1` varchar(255) NOT NULL,
  `phoneNumber2` varchar(255) NOT NULL,
  `faxNumber` varchar(255) NOT NULL,
  `mobileNumber` varchar(255) NOT NULL,
  `emailAddress` varchar(255) NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`ContactId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `contactdatacategory` (
  `categoryid` int(11) NOT NULL AUTO_INCREMENT,
  `colourHex` varchar(255) NOT NULL,
  `title` varchar(1024) NOT NULL,
  `description` varchar(1024) NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`categoryid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `contactlinktocategory` (
  `ContactLinkToCategoryId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `ContactId` int(10) unsigned NOT NULL,
  `CategoryId` int(10) unsigned NOT NULL,
  PRIMARY KEY (`ContactLinkToCategoryId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `contacts` (
  `ContactsId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `numColumnsToShow` int(10) unsigned NOT NULL,
  `maxNumContactsPerColumn` int(10) unsigned NOT NULL,
  `nameDisplayMode` varchar(255) NOT NULL,
  `accessLevelToEditContacts` varchar(255) NOT NULL,
  `accessLevelToAddContacts` varchar(255) NOT NULL,
  `deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`ContactsId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `flashobject` (
  `FlashObjectId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `SWFPath` varchar(255) NOT NULL DEFAULT '',
  `DisplayWidth` int(11) NOT NULL,
  `DisplayHeight` int(11) NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`FlashObjectId`),
  KEY `flashobject_secondary` (`PageId`,`Identifier`,`Deleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE  `glossary` (
  `glossaryid` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `pageid` int(10) unsigned NOT NULL,
  `identifier` int(10) unsigned NOT NULL,
  `langShortCode` varchar(255) NOT NULL,
  `sortOrder` varchar(255) NOT NULL,
  `ViewMode` varchar(255) NOT NULL,
  `deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`glossaryid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `glossarydata` (
  `GlossaryDataId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `phGlossaryId` int(10) unsigned NOT NULL,
  `isAcronym` int(10) unsigned NOT NULL,
  `word` varchar(255) NOT NULL,
  `description` text NOT NULL,
  `deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`GlossaryDataId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `googlemap` (
  `GoogleMapId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `APIKey` varchar(255) NOT NULL,
  `PopupHtml` varchar(255) NOT NULL,
  `Latitude` double NOT NULL,
  `Longitude` double NOT NULL,
  `intitialZoomLevel` int(10) unsigned NOT NULL DEFAULT '13',
  `MapType` varchar(50) NOT NULL DEFAULT 'G_NORMAL_MAP',
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`GoogleMapId`),
  KEY `googlemap_secondary` (`PageId`,`Identifier`,`Deleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE  `htmlcontent` (
  `HtmlContentId` int(11) NOT NULL AUTO_INCREMENT,
  `PageId` int(11) NOT NULL DEFAULT '0',
  `Identifier` int(11) NOT NULL DEFAULT '0',
  `langShortCode` varchar(255) NOT NULL DEFAULT '',
  `RevisionNumber` int(11) NOT NULL DEFAULT '1',
  `html` longtext NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`HtmlContentId`),
  KEY `htmlcontent_secondary` (`PageId`,`Identifier`,`Deleted`,`langShortCode`) USING BTREE
) ENGINE=InnoDB  DEFAULT CHARSET=utf8;


CREATE TABLE  `imagegallery` (
  `ImageGalleryId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `subDir` varchar(255) NOT NULL,
  `thumbSize` int(11) NOT NULL,
  `largeSize` int(11) NOT NULL,
  `numThumbsPerRow` int(11) NOT NULL,
  `deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`ImageGalleryId`),
  KEY `imagegallery_secondary` (`PageId`,`Identifier`,`deleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `imagegalleryimages` (
  `ImageGalleryImageId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Caption` varchar(255) NOT NULL,
  `Filename` varchar(255) NOT NULL,
  `ImageGalleryId` int(10) unsigned NOT NULL,
  PRIMARY KEY (`ImageGalleryImageId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `jobdetails` (
  `JobId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `langShortCode` varchar(255) NOT NULL,
  `JobLocationId` int(10) NOT NULL,
  `RemoveAnonAccessAt` datetime NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`JobId`),
  KEY `JobDetailsIndex` (`PageId`,`Identifier`,`langShortCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE  `joblocations` (
  `JobLocationId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `LocationText` text NOT NULL,
  `IsAllLocations` int(10) unsigned NOT NULL DEFAULT '0',
  `SortOrdinal` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`JobLocationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE  `jobsummary` (
  `JobSummaryId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `langShortCode` varchar(255) NOT NULL,
  `locationId` int(11) NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`JobSummaryId`),
  KEY `jobsummary_secondary` (`PageId`,`Identifier`,`Deleted`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8;


CREATE TABLE  `pagefileitem` (
  `PageFileItemId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `langShortCode` varchar(255) NOT NULL,
  `Filename` varchar(255) NOT NULL,
  `Title` varchar(255) NOT NULL,
  `Author` varchar(255) NOT NULL DEFAULT '',
  `Abstract` text NOT NULL,
  `FileSize` int(10) unsigned NOT NULL,
  `LastModified` datetime NOT NULL,
  `CreatorUsername` varchar(255) NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`PageFileItemId`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8;


CREATE TABLE  `pagefiles` (
  `PageFilesId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `langShortCode` varchar(255) NOT NULL,
  `sortDirection` varchar(50) NOT NULL,
  `sortColumn` varchar(50) NOT NULL,
  `tabularDisplayLinkMode` varchar(50) NOT NULL,
  `numFilesToShowPerPage` int(11) NOT NULL,
  `accessLevelToAddFiles` varchar(50) NOT NULL,
  `accessLevelToEditFiles` varchar(50) NOT NULL,
  `deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`PageFilesId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `pagelanginfo` (
  `pageId` int(10) unsigned NOT NULL,
  `langCode` varchar(255) NOT NULL,
  `name` varchar(255) DEFAULT NULL,
  `title` varchar(255) NOT NULL,
  `menuTitle` varchar(255) NOT NULL,
  `searchEngineDescription` text NOT NULL,
  PRIMARY KEY (`pageId`,`langCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `pagelocks` (
  `pageid` int(11) NOT NULL,
  `LockedByUsername` varchar(255) NOT NULL,
  `LockExpiresAt` datetime NOT NULL,
  PRIMARY KEY (`pageid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `pagenotfoundredirect` (
  `PageNotFoundRedirectId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `requestedUrl` mediumtext NOT NULL,
  `redirectToPageId` int(10) unsigned NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`PageNotFoundRedirectId`),
  KEY `pagenotfoundredirect_secondary` (`requestedUrl`(255),`Deleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE  `pageredirect` (
  `PageRedirectId` int(11) NOT NULL AUTO_INCREMENT,
  `PageId` int(11) NOT NULL DEFAULT '0',
  `Identifier` int(11) NOT NULL DEFAULT '0',
  `langShortCode` varchar(6) NOT NULL,
  `url` varchar(1024) NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`PageRedirectId`),
  KEY `PageId` (`PageId`,`Identifier`),
  KEY `pageredirect_secondary` (`PageId`,`Identifier`,`Deleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `pagerevisiondata` (
  `PageId` int(11) NOT NULL,
  `RevisionNumber` int(11) NOT NULL,
  `ModificationDate` datetime NOT NULL,
  `ModifiedBy` varchar(255) NOT NULL,
  PRIMARY KEY (`PageId`,`RevisionNumber`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `pages` (
  `pageId` int(11) NOT NULL AUTO_INCREMENT,
  `showInMenu` int(10) unsigned NOT NULL DEFAULT '1',
  `template` varchar(255) NOT NULL,
  `parentPageId` int(11) NOT NULL DEFAULT '0',
  `SortOrdinal` int(11) NOT NULL DEFAULT '0',
  `CreatedDateTime` datetime NOT NULL,
  `LastUpdatedDateTime` datetime NOT NULL,
  `LastModifiedBy` varchar(255) NOT NULL DEFAULT '',
  `RevisionNumber` int(11) NOT NULL DEFAULT '1',
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`pageId`),
  KEY `pages_secondary` (`pageId`,`Deleted`),
  KEY `pages_tertiary` (`parentPageId`,`Deleted`),
  KEY `pages_quartinary` (`parentPageId`,`Deleted`) USING BTREE,
  KEY `pages_deleted` (`Deleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE  TABLE `persistentvariables` (  `PersistentVariableId` INT NOT NULL AUTO_INCREMENT ,  `Name` VARCHAR(255) NOT NULL ,  `PersistedValue` BLOB NULL ,  PRIMARY KEY (`PersistentVariableId`) ,  UNIQUE INDEX `Name_UNIQUE` (`Name` ASC) );

CREATE TABLE  `plaintextcontent` (
  `PlainTextContentId` int(11) NOT NULL AUTO_INCREMENT,
  `PageId` int(11) NOT NULL DEFAULT '0',
  `Identifier` int(11) NOT NULL DEFAULT '0',
  `langShortCode` varchar(255) NOT NULL,
  `plaintext` longtext NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`PlainTextContentId`),
  KEY `plaintextcontent_secondary` (`PageId`,`Identifier`,`Deleted`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8;


CREATE TABLE  `resourceitemmetadata` (
  `AutoIncId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `ResourceId` int(10) unsigned NOT NULL,
  `ResourceRevisionNumber` int(10) unsigned NOT NULL,
  `Name` varchar(255) NOT NULL,
  `Value` longtext NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`AutoIncId`),
  KEY `ResourceId` (`ResourceId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `resourceitems` (
  `AutoIncId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `ResourceId` int(11) NOT NULL,
  `RevisionNumber` int(11) NOT NULL,
  `Filename` varchar(255) NOT NULL,
  `FilePath` text NOT NULL,
  `FileDirectory` text NOT NULL,
  `FileSize` int(10) unsigned NOT NULL,
  `FileTimestamp` datetime NOT NULL,
  `MimeType` varchar(255) NOT NULL,
  `ModifiedBy` varchar(255) NOT NULL,
  `ModificationDate` datetime NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`AutoIncId`),
  UNIQUE KEY `ResourceItemsUniqueIdRevisionNumber` (`ResourceId`,`RevisionNumber`),
  KEY `RevisionNumIndex` (`RevisionNumber`,`FileDirectory`(255),`Deleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `singleimage` (
  `SingleImageId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `langShortCode` varchar(255) NOT NULL DEFAULT '',
  `RevisionNumber` int(11) NOT NULL DEFAULT '1',
  `ImagePath` varchar(255) NOT NULL,
  `ThumbnailDisplayBoxWidth` int(11) NOT NULL DEFAULT '-1',
  `ThumbnailDisplayBoxHeight` int(11) NOT NULL DEFAULT '-1',
  `FullSizeDisplayBoxWidth` int(11) NOT NULL DEFAULT '-1',
  `FullSizeDisplayBoxHeight` int(11) NOT NULL DEFAULT '-1',
  `Caption` varchar(255) NOT NULL,
  `Credits` varchar(255) NOT NULL,
  `Tags` varchar(255) NOT NULL DEFAULT '',
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`SingleImageId`),
  KEY `singleimage_secondary` (`PageId`,`Identifier`,`Deleted`,`langShortCode`) USING BTREE,
  KEY `singleimage_tertiary` (`RevisionNumber`,`PageId`,`langShortCode`,`ImagePath`,`Deleted`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8;


# ALTER TABLE `singleimage` ADD INDEX `singleimage_tertiary`(`RevisionNumber`, `PageId`, `langShortCode`, `ImagePath`, `Deleted`);

CREATE TABLE  `singleimagegallery` (
  `SingleImageGalleryId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `langShortCode` varchar(255) NOT NULL,
  `PageIdToGatherImagesFrom` int(10) NOT NULL,
  `RecursiveGatherImages` int(10) unsigned NOT NULL,
  `ThumbnailDisplayBoxWidth` int(11) NOT NULL,
  `ThumbnailDisplayBoxHeight` int(11) NOT NULL,
  `OverrideFullDisplayBoxSize` int(10) unsigned NOT NULL,
  `FullSizeDisplayBoxWidth` int(11) NOT NULL,
  `FullSizeDisplayBoxHeight` int(11) NOT NULL,
  `NumThumbsPerRow` int(10) unsigned NOT NULL,
  `NumThumbsPerPage` int(11) NOT NULL,
  `ShowOnlyTags` varchar(255) NOT NULL DEFAULT '',
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`SingleImageGalleryId`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8;


CREATE TABLE  `userfeedbackform` (
  `pageid` int(10) unsigned NOT NULL,
  `identifier` int(10) unsigned NOT NULL,
  `LangCode` varchar(5) NOT NULL,
  `EmailAddressesToNotify` text NOT NULL,
  `ThankyouMessage` text NOT NULL,
  `FormFieldDisplayWidth` int(10) unsigned NOT NULL,
  `TextAreaQuestion` varchar(255) NOT NULL,
  PRIMARY KEY (`pageid`,`identifier`,`LangCode`),
  KEY `userfeedbackform_1` (`pageid`,`identifier`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

# ALTER TABLE `userfeedbackform` ADD INDEX `userfeedbackform_1`(`pageid`, `identifier`);


CREATE TABLE  `userfeedbacksubmitteddata` (
  `UserFeedbackSubmittedDataId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `dateTimeSubmitted` datetime NOT NULL,
  `Name` varchar(255) NOT NULL,
  `EmailAddress` varchar(255) NOT NULL,
  `Location` varchar(255) NOT NULL,
  `TextAreaQuestion` varchar(255) NOT NULL,
  `TextAreaValue` text NOT NULL,
  `ReferringUrl` text NOT NULL,
  PRIMARY KEY (`UserFeedbackSubmittedDataId`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8;


CREATE TABLE `NewsArticleAggregator` (
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `LangCode` varchar(2) NOT NULL,
  `DefaultYearToDisplay` int(11) NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `NewsArticleDetails` (
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `LangCode` varchar(2) NOT NULL,
  `DateOfNews` datetime DEFAULT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `EventCalendarAggregator` (
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `LangCode` varchar(2) NOT NULL,
  `ViewMode` varchar(50) NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `eventcalendardetails` (
  `PageId` int(11) NOT NULL,
  `Identifier` int(11) NOT NULL,
  `LangCode` varchar(2) NOT NULL,
  `Description` text NOT NULL,
  `CategoryId` int(11) NOT NULL,
  `StartDateTime` datetime NOT NULL,
  `EndDateTime` datetime NOT NULL,
  `CreatedBy` varchar(255) NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `eventcalendarcategory` (
  `CategoryId` int(11) NOT NULL,
  `LangCode` varchar(2) NOT NULL,
  `ColorHex` varchar(7) NOT NULL,
  `Title` varchar(255) NOT NULL,
  `Description` text NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`CategoryId`,`LangCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `RegisterProject` (
  `ProjectId` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `Location` varchar(255) NOT NULL,
  `Description` text NOT NULL,
  `ContactPerson` varchar(255) NOT NULL,
  `Email` varchar(255) NOT NULL,
  `Telephone` varchar(30) NOT NULL,
  `Cellphone` varchar(30) NOT NULL,
  `Website` varchar(255) NOT NULL,
  `FundingSource` varchar(255) NOT NULL,
  `CreatedDateTime` datetime DEFAULT NULL,
  `ClientIP` varchar(255) NOT NULL,
  PRIMARY KEY (`ProjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `userimagegallery` (
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `LangCode` varchar(5) NOT NULL,
  `NumThumbsPerPage` int(10) NOT NULL,
  `NumThumbsPerRow` int(10) NOT NULL,
  `ThumbnailDisplayBoxWidth` int(10) NOT NULL,
  `ThumbnailDisplayBoxHeight` int(10) NOT NULL,
  `FullSizeDisplayBoxWidth` int(10) NOT NULL,
  `FullSizeDisplayBoxHeight` int(10) NOT NULL,
  `FullSizeLinkMode` varchar(255) NOT NULL,
  `CaptionDisplayLocation` varchar(255) NOT NULL,
  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8;


CREATE TABLE `procurementaggregator` (
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `LangCode` varchar(5) NOT NULL,
  `DefaultYearToDisplay` int(11) NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `procurementdetails` (
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `LangCode` varchar(5) NOT NULL,
  `DateOfProcurement` datetime DEFAULT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `zone` (
  `ZoneId` int(11) NOT NULL AUTO_INCREMENT,
  `StartingPageId` int(11) NOT NULL,
  `ZoneName` varchar(255) NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`ZoneId`),
  KEY `StartingPageId` (`StartingPageId`),
  FOREIGN KEY (`StartingPageId`) REFERENCES `pages` (`pageId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `ZoneUserRole` (
  `ZoneId` int(11) NOT NULL,
  `UserRoleId` int(11) NOT NULL,
  `ReadAccess` int(1) DEFAULT '0',
  `WriteAccess` int(1) DEFAULT '0',
  PRIMARY KEY (`ZoneId`,`UserRoleId`),
  FOREIGN KEY (`ZoneId`) REFERENCES `zone` (`ZoneId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `filelibrarycategory` (
  `CategoryId` int(11) NOT NULL,
  `LangCode` varchar(5) NOT NULL,
  `EventRequired` int(1) NOT NULL DEFAULT '0',
  `CategoryName` varchar(255) NOT NULL,
  `SortOrdinal` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`CategoryId`,`LangCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE `FileLibraryAggregator` (
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `LangCode` varchar(5) NOT NULL,
  `NumFilesOverview` int(11) NOT NULL,
  `NumFilesPerPage` int(11) NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


CREATE TABLE  `filelibrarydetails` (
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `LangCode` varchar(5) NOT NULL,
  `Filename` varchar(255) NOT NULL,
  `CategoryId` int(11) NOT NULL,
  `Author` varchar(255) NOT NULL DEFAULT '',
  `Description` text NOT NULL,
  `LastModified` datetime NOT NULL,
  `CreatedBy` varchar(255) NOT NULL,
  `EventPageId` int(11) NOT NULL DEFAULT '-1',
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE  `filelibraryaggregator2` (
  `SimpleFileAggregatorId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `PageId` int(10) unsigned NOT NULL,
  `Identifier` int(10) unsigned NOT NULL,
  `LangCode` varchar(5) NOT NULL,
  `LinkedPageId` int(11) NOT NULL,
  `LinkedIdentifier` int(11) NOT NULL,
  `LinkedLangCode` varchar(5) NOT NULL,
  `Deleted` datetime DEFAULT NULL,
  PRIMARY KEY (`SimpleFileAggregatorId`),
  KEY `simplefileaggregatorPageIndex` (`PageId`,`Identifier`,`LangCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

