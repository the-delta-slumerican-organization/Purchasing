﻿ALTER TABLE [dbo].[Orders]
    ADD CONSTRAINT [FK_Orders_OrderStatusCodes] FOREIGN KEY ([OrderStatusId]) REFERENCES [dbo].[OrderStatusCodes] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
