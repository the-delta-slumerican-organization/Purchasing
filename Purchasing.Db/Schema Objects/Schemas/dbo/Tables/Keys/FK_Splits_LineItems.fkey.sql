﻿ALTER TABLE [dbo].[Splits]
    ADD CONSTRAINT [FK_Splits_LineItems] FOREIGN KEY ([LineItemId]) REFERENCES [dbo].[LineItems] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;

