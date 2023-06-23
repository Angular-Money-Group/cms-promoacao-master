CREATE TABLE IF NOT EXISTS `btz_orderpayment` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `IdOrder` int(11) NOT NULL,
  `RequestId` varchar(50) DEFAULT NULL,
  `RequestStatus` int(11) NOT NULL,
  `Gateway` int(11) NOT NULL,
  `CustomerFirstName` varchar(255) DEFAULT NULL,
  `CustomerLastName` varchar(255) DEFAULT NULL,
  `CustomerEmail` varchar(255) DEFAULT NULL,
  `CustomerPhone` varchar(40) DEFAULT NULL,
  `CustomerAddressZip` varchar(20) DEFAULT NULL,
  `CustomerAddressPublicPlace` varchar(255) DEFAULT NULL,
  `CustomerAddressNumber` varchar(40) DEFAULT NULL,
  `CustomerAddressNeighborhood` varchar(255) DEFAULT NULL,
  `CustomerAddressCity` varchar(255) DEFAULT NULL,
  `CustomerAddressState` varchar(255) DEFAULT NULL,
  `CustomerAddressCountry` varchar(255) DEFAULT NULL,
  `OrderAmount` decimal(18,2) NOT NULL,
  `OrderOperationType` varchar(30) DEFAULT NULL,
  `PaymentCardHolder` varchar(50) DEFAULT NULL,
  `PaymentPaymentId` varchar(50) DEFAULT NULL,
  `PaymentInstallments` int(11) NOT NULL,
  `PaymentAuthCode` varchar(80) DEFAULT NULL,
  `PaymentNsu` varchar(80) DEFAULT NULL,
  `PaymentDescription` varchar(255) DEFAULT NULL,
  `PaymentCardBrand` varchar(40) DEFAULT NULL,
  `PaymentLastFourDigits` varchar(20) DEFAULT NULL,
  `PaymentUrl` varchar(255) DEFAULT NULL,
  `PaymentQrCode` longtext,
  `PaymentBarCode` varchar(80) DEFAULT NULL,
  `CreatedAt` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_IdOrder` (`IdOrder`),
  CONSTRAINT `FK_btz_orderpayment_btz_order_IdOrder` FOREIGN KEY (`IdOrder`) REFERENCES `btz_order` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;