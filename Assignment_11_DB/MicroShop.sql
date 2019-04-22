drop database if exists MicroShop;
create database `MicroShop` /*!40100 default character set latin1 */;


use `MicroShop`;

drop table if exists `Customer`;

create table `Customer` (
id int not null AUTO_INCREMENT primary key,
name text not null
);

drop table if exists `Product`;

create table `Product` (
id int not null AUTO_INCREMENT primary key,
name text not null,
price decimal(18,4) not null
);

drop table if exists `Order`;

create table `Order` (
id int not null AUTO_INCREMENT primary key,
date text not null,
total decimal(18,4) not null,
customerId int not null,
key `Customer` (`CustomerId`),
constraint OrderForeignKey_0 foreign key (CustomerId) references `Customer` (id)
);

drop table if exists `OrderLine`;

create table `OrderLine` (
id int not null AUTO_INCREMENT primary key,
orderId int not null,
productId int not null,
count decimal(18,4) not null,
total decimal(18,4) not null,
key `Order` (`OrderId`),
constraint OrderLineForeignKey_0 foreign key (OrderId) references `Order` (id),
key `Product` (`ProductId`),
constraint OrderLineForeignKey_1 foreign key (ProductId) references `Product` (id)
);