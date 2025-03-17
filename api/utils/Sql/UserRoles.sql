SELECT au.UserName, au.Id as UserId, r.Id as RoleId, r.Name as RoleName
FROM AppUser as au
FULL OUTER JOIN AspNetUserRoles as ur
	ON au.Id = ur.UserId
FULL OUTER JOIN AspNetRoles as r
	ON r.Id = ur.RoleId