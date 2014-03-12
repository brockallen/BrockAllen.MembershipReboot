INSERT [dbo].[UserAccounts] ([ID], [Tenant], [Username], [Email], [Created], [LastUpdated], [PasswordChanged], [RequiresPasswordReset], [MobileCode], [MobileCodeSent], [MobilePhoneNumber], [AccountTwoFactorAuthMode], [CurrentTwoFactorAuthStatus], [IsAccountVerified], [IsLoginAllowed], [IsAccountClosed], [AccountClosed], [LastLogin], [LastFailedLogin], [FailedLoginCount], [VerificationKey], [VerificationPurpose], [VerificationKeySent], [HashedPassword], [LastFailedPasswordReset], [FailedPasswordResetCount], [MobilePhoneNumberChanged], [VerificationStorage]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'default', N'BrockAllen', N'brockallen@gmail.com', CAST(0x0000A2E500207CE4 AS DateTime), CAST(0x0000A2E500227CB0 AS DateTime), CAST(0x0000A2E50020DF7D AS DateTime), 0, NULL, NULL, NULL, 0, 0, 1, 1, 0, NULL, CAST(0x0000A2E500227CB0 AS DateTime), NULL, 0, NULL, NULL, NULL, N'1F400.AHX3oMy62CHEpgJJNsxua/FxiMJ8O5WfZRfBcKUQF1ohqopuZ6iACNPSqX+EkIYMhg==', NULL, 0, NULL, NULL)
GO
INSERT [dbo].[UserCertificates] ([UserAccountID], [Thumbprint], [Subject]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'B020C3F6C4C61FB3BF8769CFE28D70C7E8A70C78', N'CN=ballen')
GO
INSERT [dbo].[UserClaims] ([UserAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender', N'female')
GO
INSERT [dbo].[UserClaims] ([UserAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender', N'male')
GO
INSERT [dbo].[LinkedAccounts] ([UserAccountID], [ProviderName], [ProviderAccountID], [LastLogin]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Facebook', N'655646132', CAST(0x0000A2E500210F6F AS DateTime))
GO
INSERT [dbo].[LinkedAccounts] ([UserAccountID], [ProviderName], [ProviderAccountID], [LastLogin]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Google', N'https://www.google.com/accounts/o8/id?id=AItOawlfdD0GNOSOPIIrxlPKpRQWq-Zr6b2Kgrc', CAST(0x0000A2E50020932C AS DateTime))
GO
INSERT [dbo].[LinkedAccounts] ([UserAccountID], [ProviderName], [ProviderAccountID], [LastLogin]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Google', N'https://www.google.com/accounts/o8/id?id=AItOawlo2uBjcTJp2J5ZUhlTyRhLiQ2zND3x_28', CAST(0x0000A2E500227CAF AS DateTime))
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Facebook', N'655646132', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress', N'brockallen@gmail.com')
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Facebook', N'655646132', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name', N'brock.allen.180')
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Facebook', N'655646132', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier', N'655646132')
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Facebook', N'655646132', N'urn:facebook:link', N'https://www.facebook.com/brock.allen.180')
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Facebook', N'655646132', N'urn:facebook:name', N'Brock Allen')
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Google', N'https://www.google.com/accounts/o8/id?id=AItOawlfdD0GNOSOPIIrxlPKpRQWq-Zr6b2Kgrc', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress', N'brockallen@gmail.com')
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Google', N'https://www.google.com/accounts/o8/id?id=AItOawlfdD0GNOSOPIIrxlPKpRQWq-Zr6b2Kgrc', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname', N'Brock')
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Google', N'https://www.google.com/accounts/o8/id?id=AItOawlfdD0GNOSOPIIrxlPKpRQWq-Zr6b2Kgrc', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name', N'Brock Allen')
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Google', N'https://www.google.com/accounts/o8/id?id=AItOawlfdD0GNOSOPIIrxlPKpRQWq-Zr6b2Kgrc', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier', N'https://www.google.com/accounts/o8/id?id=AItOawlfdD0GNOSOPIIrxlPKpRQWq-Zr6b2Kgrc')
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Google', N'https://www.google.com/accounts/o8/id?id=AItOawlfdD0GNOSOPIIrxlPKpRQWq-Zr6b2Kgrc', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname', N'Allen')
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Google', N'https://www.google.com/accounts/o8/id?id=AItOawlo2uBjcTJp2J5ZUhlTyRhLiQ2zND3x_28', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress', N'brockallentest@gmail.com')
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Google', N'https://www.google.com/accounts/o8/id?id=AItOawlo2uBjcTJp2J5ZUhlTyRhLiQ2zND3x_28', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname', N'Brock')
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Google', N'https://www.google.com/accounts/o8/id?id=AItOawlo2uBjcTJp2J5ZUhlTyRhLiQ2zND3x_28', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name', N'Brock Allen')
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Google', N'https://www.google.com/accounts/o8/id?id=AItOawlo2uBjcTJp2J5ZUhlTyRhLiQ2zND3x_28', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier', N'https://www.google.com/accounts/o8/id?id=AItOawlo2uBjcTJp2J5ZUhlTyRhLiQ2zND3x_28')
GO
INSERT [dbo].[LinkedAccountClaims] ([UserAccountID], [ProviderName], [ProviderAccountID], [Type], [Value]) VALUES (N'56ed9544-4d7d-4fed-930b-a80ca69b3e05', N'Google', N'https://www.google.com/accounts/o8/id?id=AItOawlo2uBjcTJp2J5ZUhlTyRhLiQ2zND3x_28', N'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname', N'Allen')
GO
INSERT [dbo].[Groups] ([ID], [Tenant], [Name], [Created], [LastUpdated]) VALUES (N'cff5585b-d47a-40d9-aabc-03225f804190', N'default', N'a', CAST(0x0000A2E500295776 AS DateTime), CAST(0x0000A2E500299312 AS DateTime))
GO
INSERT [dbo].[Groups] ([ID], [Tenant], [Name], [Created], [LastUpdated]) VALUES (N'45b5fc05-8794-445c-a8e7-0436610c53b8', N'default', N'baa', CAST(0x0000A2E5002981E4 AS DateTime), CAST(0x0000A2E50029E562 AS DateTime))
GO
INSERT [dbo].[Groups] ([ID], [Tenant], [Name], [Created], [LastUpdated]) VALUES (N'06e2c43b-0079-48f4-85da-04a94e891231', N'default', N'baaa', CAST(0x0000A2E500298597 AS DateTime), CAST(0x0000A2E500298597 AS DateTime))
GO
INSERT [dbo].[Groups] ([ID], [Tenant], [Name], [Created], [LastUpdated]) VALUES (N'8f4a3ad9-7605-4eef-9119-153122ef5b79', N'default', N'd', CAST(0x0000A2E500295E14 AS DateTime), CAST(0x0000A2E500295E14 AS DateTime))
GO
INSERT [dbo].[Groups] ([ID], [Tenant], [Name], [Created], [LastUpdated]) VALUES (N'5d81b6b0-3066-4341-b942-28409ef345d6', N'default', N'b', CAST(0x0000A2E50029598A AS DateTime), CAST(0x0000A2E50029C9FC AS DateTime))
GO
INSERT [dbo].[Groups] ([ID], [Tenant], [Name], [Created], [LastUpdated]) VALUES (N'4336a6bb-38e8-4fda-a10a-36ba30c463a3', N'default', N'aab', CAST(0x0000A2E50029739E AS DateTime), CAST(0x0000A2E50029739E AS DateTime))
GO
INSERT [dbo].[Groups] ([ID], [Tenant], [Name], [Created], [LastUpdated]) VALUES (N'a39038ce-1836-4944-9c07-39105742db6b', N'default', N'ab', CAST(0x0000A2E5002968B2 AS DateTime), CAST(0x0000A2E50029AA9F AS DateTime))
GO
INSERT [dbo].[Groups] ([ID], [Tenant], [Name], [Created], [LastUpdated]) VALUES (N'cfdf43c7-333b-4103-8118-5d3185d13e58', N'default', N'bb', CAST(0x0000A2E500297F26 AS DateTime), CAST(0x0000A2E500297F26 AS DateTime))
GO
INSERT [dbo].[Groups] ([ID], [Tenant], [Name], [Created], [LastUpdated]) VALUES (N'e4b17ea2-f5c4-4dbe-aa01-841a8e6f6577', N'default', N'aa', CAST(0x0000A2E500296072 AS DateTime), CAST(0x0000A2E500299C49 AS DateTime))
GO
INSERT [dbo].[Groups] ([ID], [Tenant], [Name], [Created], [LastUpdated]) VALUES (N'7c7ba003-9ed6-4de2-af63-883897ca5e77', N'default', N'ba', CAST(0x0000A2E500297C86 AS DateTime), CAST(0x0000A2E50029DD9A AS DateTime))
GO
INSERT [dbo].[Groups] ([ID], [Tenant], [Name], [Created], [LastUpdated]) VALUES (N'394d4aef-1142-4047-84ba-badaeea44ce7', N'default', N'c', CAST(0x0000A2E500295B65 AS DateTime), CAST(0x0000A2E500295B65 AS DateTime))
GO
INSERT [dbo].[Groups] ([ID], [Tenant], [Name], [Created], [LastUpdated]) VALUES (N'86cd16bb-fd25-4c99-8684-d2bef897ec54', N'default', N'ac', CAST(0x0000A2E500296BC9 AS DateTime), CAST(0x0000A2E500296BC9 AS DateTime))
GO
INSERT [dbo].[Groups] ([ID], [Tenant], [Name], [Created], [LastUpdated]) VALUES (N'a943e3e7-eed2-4ec1-b4db-ed0d60226065', N'default', N'aba', CAST(0x0000A2E50029781A AS DateTime), CAST(0x0000A2E50029781A AS DateTime))
GO
INSERT [dbo].[Groups] ([ID], [Tenant], [Name], [Created], [LastUpdated]) VALUES (N'3fbb39df-917e-4d6c-81a0-fd2c892dcdd4', N'default', N'aaa', CAST(0x0000A2E5002970CF AS DateTime), CAST(0x0000A2E5002970CF AS DateTime))
GO
INSERT [dbo].[GroupChilds] ([GroupID], [ChildGroupID]) VALUES (N'cff5585b-d47a-40d9-aabc-03225f804190', N'a39038ce-1836-4944-9c07-39105742db6b')
GO
INSERT [dbo].[GroupChilds] ([GroupID], [ChildGroupID]) VALUES (N'cff5585b-d47a-40d9-aabc-03225f804190', N'e4b17ea2-f5c4-4dbe-aa01-841a8e6f6577')
GO
INSERT [dbo].[GroupChilds] ([GroupID], [ChildGroupID]) VALUES (N'cff5585b-d47a-40d9-aabc-03225f804190', N'86cd16bb-fd25-4c99-8684-d2bef897ec54')
GO
INSERT [dbo].[GroupChilds] ([GroupID], [ChildGroupID]) VALUES (N'45b5fc05-8794-445c-a8e7-0436610c53b8', N'06e2c43b-0079-48f4-85da-04a94e891231')
GO
INSERT [dbo].[GroupChilds] ([GroupID], [ChildGroupID]) VALUES (N'5d81b6b0-3066-4341-b942-28409ef345d6', N'cfdf43c7-333b-4103-8118-5d3185d13e58')
GO
INSERT [dbo].[GroupChilds] ([GroupID], [ChildGroupID]) VALUES (N'5d81b6b0-3066-4341-b942-28409ef345d6', N'7c7ba003-9ed6-4de2-af63-883897ca5e77')
GO
INSERT [dbo].[GroupChilds] ([GroupID], [ChildGroupID]) VALUES (N'a39038ce-1836-4944-9c07-39105742db6b', N'a943e3e7-eed2-4ec1-b4db-ed0d60226065')
GO
INSERT [dbo].[GroupChilds] ([GroupID], [ChildGroupID]) VALUES (N'e4b17ea2-f5c4-4dbe-aa01-841a8e6f6577', N'4336a6bb-38e8-4fda-a10a-36ba30c463a3')
GO
INSERT [dbo].[GroupChilds] ([GroupID], [ChildGroupID]) VALUES (N'e4b17ea2-f5c4-4dbe-aa01-841a8e6f6577', N'3fbb39df-917e-4d6c-81a0-fd2c892dcdd4')
GO
INSERT [dbo].[GroupChilds] ([GroupID], [ChildGroupID]) VALUES (N'7c7ba003-9ed6-4de2-af63-883897ca5e77', N'45b5fc05-8794-445c-a8e7-0436610c53b8')
GO
INSERT [dbo].[GroupChilds] ([GroupID], [ChildGroupID]) VALUES (N'7c7ba003-9ed6-4de2-af63-883897ca5e77', N'cfdf43c7-333b-4103-8118-5d3185d13e58')
GO
