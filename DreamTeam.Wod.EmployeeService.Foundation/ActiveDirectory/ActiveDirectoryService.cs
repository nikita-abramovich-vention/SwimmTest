using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Controls;

namespace DreamTeam.Wod.EmployeeService.Foundation.ActiveDirectory
{
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        private const string DomainNamePropertyName = "samaccountname";
        private const string PasswordChangeDatePropertyName = "pwdlastset";


        private readonly ActiveDirectoryAuthenticationOptions _options;


        public ActiveDirectoryService(IOptions<ActiveDirectoryAuthenticationOptions> options)
        {
            _options = options.Value;
        }


        public async Task<OperationResult<ActiveDirectoryUser>> GetUserAsync(string domainName)
        {
            var getUsersResult = await GetUsersAsync();
            if (!getUsersResult.IsSuccessful)
            {
                return OperationResult<ActiveDirectoryUser>.CreateUnsuccessful();
            }

            var users = getUsersResult.Result;

            return users.SingleOrDefault(u => u.DomainName == domainName);
        }

        public async Task<OperationResult<IReadOnlyCollection<ActiveDirectoryUser>>> GetUsersAsync()
        {
            var activeDirectories = _options.Servers.Values.Where(s => s.IsEnabled);
            var getUsersTasks = activeDirectories.Select(GetUsersAsync).ToList();
            var getUsersResults = await Task.WhenAll(getUsersTasks);
            if (getUsersResults.Any(r => !r.IsSuccessful))
            {
                return OperationResult<IReadOnlyCollection<ActiveDirectoryUser>>.CreateUnsuccessful();
            }

            var activeDirectoryUsers = getUsersResults.SelectMany(r => r.Result).ToList();

            return activeDirectoryUsers;
        }

        private static Task<OperationResult<IReadOnlyCollection<ActiveDirectoryUser>>> GetUsersAsync(ActiveDirectoryServerOptions activeDirectoryServerOptions)
        {
            return Task.Run(
                () =>
            {
                using var ldapConnection = new LdapConnection();
                var ldapUri = new Uri(activeDirectoryServerOptions.Path);
                try
                {
                    ldapConnection.Connect(ldapUri.Host, ldapUri.Port);
                }
                catch (Exception ex) when (ex.IsCatchableExceptionType())
                {
                    LoggerContext.Current.LogError("Failed to connect to the AD server at {activeDirectoryPath} to get users.", ex, activeDirectoryServerOptions.Path);

                    return OperationResult<IReadOnlyCollection<ActiveDirectoryUser>>.CreateUnsuccessful();
                }

                try
                {
                    var ldapPath = ldapUri.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
                    var domainDame =
                        ldapPath.Split(',').Select(pair => pair.Split('=')).First(pair => pair[0] == "DC")[1];
                    ldapConnection.Bind($"{domainDame}\\{activeDirectoryServerOptions.Username}", activeDirectoryServerOptions.Password);
                    var cons = ldapConnection.SearchConstraints;
                    cons.ReferralFollowing = true;

                    var i = 0;
                    var page = 1;
                    var users = new List<ActiveDirectoryUser>();
                    while (true)
                    {
                        AddPagination(cons, page, cons.MaxResults);
                        var result = ldapConnection.Search(
                            ldapPath,
                            LdapConnection.ScopeSub,
                            "(objectcategory=Person)",
                            new[] { DomainNamePropertyName, PasswordChangeDatePropertyName },
                            false,
                            cons);
                        while (result.HasMore() && i < cons.MaxResults)
                        {
                            var pageResult = result.Next();
                            var attributes = pageResult.GetAttributeSet();
                            var name = attributes.GetValueOrDefault(DomainNamePropertyName, null)?.StringValue;
                            var pwdLastSet = attributes.GetValueOrDefault(PasswordChangeDatePropertyName, null)?.StringValue;
                            if (!String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(pwdLastSet))
                            {
                                var passwordChangeDate = DateTime.FromFileTimeUtc(long.Parse(pwdLastSet));
                                var user = new ActiveDirectoryUser
                                {
                                    DomainName = name,
                                    PasswordChangeDate = passwordChangeDate,
                                };
                                users.Add(user);
                            }

                            i++;
                        }

                        if (i != cons.MaxResults)
                        {
                            break;
                        }

                        i = 0;
                        page++;
                    }

                    return users;
                }
                catch (Exception ex) when (ex.IsCatchableExceptionType())
                {
                    LoggerContext.Current.LogWarning("Failed to read users from the AD server at {activeDirectoryPath}.", ex, activeDirectoryServerOptions.Path);

                    return OperationResult<IReadOnlyCollection<ActiveDirectoryUser>>.CreateUnsuccessful();
                }
            });
        }


        private static void AddPagination(LdapSearchConstraints constraints, int page, int pageSize)
        {
            var startIndex = (page - 1) * pageSize;
            startIndex++;
            const int beforeCount = 0;
            var afterCount = pageSize - 1;
            const int contentCount = 0;

            var ldapVirtualListControl = new LdapVirtualListControl(startIndex, beforeCount, afterCount, contentCount);
            constraints.SetControls(new LdapControl[]
            {
                new LdapSortControl(new LdapSortKey("cn"), true),
                ldapVirtualListControl,
            });
        }
    }
}