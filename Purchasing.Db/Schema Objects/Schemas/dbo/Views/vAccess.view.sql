﻿CREATE VIEW [dbo].[vAccess]
	AS 

select ROW_NUMBER() over (order by orderid) id, access.orderid, access.accessuserid, access.readaccess, access.editaccess, access.IsAway
from
	(
	-- get edit access
	select orderid, accessuserid, 1 readaccess, 1 editaccess, isaway
	from vEditAccess
	union
	(
	select orderid, accessuserid, 1 readaccess, 1 editaccess, isaway
	from vAdminOrderAccess
	except
	select orderid, accessuserid, 1 readaccess, 0 editaccess, isaway
	from vEditAccess
	)
	union
	(
	-- get user with read access not with edit access
	select orderid, accessuserid, 1 readaccess, 0 editaccess, isaway
	from vReadAccess
	except 
	(
		select orderid, accessuserid, 0 readaccess, 1 editaccess, 0
		from vAdminOrderAccess
		union
		select orderid, accessuserid, 1 readaccess, 0 editaccess, isaway
		from vEditAccess	
	)
	)
) access