SELECT TOP (1000) au.Id as userId, au.UserName, s.Title, s.CreatedDate
FROM Story as s
FULL OUTER JOIN AppUser as au
	ON s.UserId = au.Id;