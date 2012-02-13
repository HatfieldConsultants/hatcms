ALTER TABLE `hatcms`.`zoneuserrole` DROP FOREIGN KEY `zoneuserrole_ibfk_1` ;

ALTER TABLE `hatcms`.`zoneuserrole` ADD COLUMN `ZoneUserRoleId` INT NOT NULL AUTO_INCREMENT  FIRST 

, DROP PRIMARY KEY 

, ADD PRIMARY KEY (`ZoneUserRoleId`) ;


ALTER TABLE `hatcms`.`zoneuserrole` 

  ADD CONSTRAINT `zoneuserrole_zone_fk_1`

  FOREIGN KEY (`ZoneId` )

  REFERENCES `hatcms`.`zone` (`ZoneId` )

  ON DELETE CASCADE

  ON UPDATE CASCADE

, ADD INDEX `zoneuserrole_zone_fk_1` (`ZoneId` ASC) ;



ALTER TABLE `hatcms`.`zoneuserrole` 

ADD UNIQUE INDEX `ZoneId_UNIQUE` (`ZoneId`, `UserRoleId`) ;


ALTER TABLE `hatcms`.`pagelanginfo` ADD COLUMN `pagelangId` INT NOT NULL AUTO_INCREMENT  FIRST 

, DROP PRIMARY KEY 

, ADD PRIMARY KEY (`pagelangId`) 

, ADD UNIQUE INDEX `pageid_langcode_unique_constraint` (`pageId` ASC, `langCode` ASC) ;


ALTER TABLE `hatcms`.`pagerevisiondata` ADD COLUMN `PageRevisionDataId` INT NOT NULL AUTO_INCREMENT  FIRST 

, DROP PRIMARY KEY 

, ADD PRIMARY KEY (`PageRevisionDataId`) 

, ADD UNIQUE INDEX `pageid_revisionnumber_unique_constraint` (`PageRevisionDataId` ASC, `PageId` ASC) ;




