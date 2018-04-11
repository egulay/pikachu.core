using Pikachu.Core.Repository;
using Pikachu.Core.Repository.Helpers;
using Helpdesk.Data;
using Helpdesk.Data.Extensions;
using Helpdesk.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace HelpdeskTest.Sql
{
    public class HelpdeskService : PikachuDataService
    {
        public HelpdeskService()
        {
            CreateSqlServiceProvider<HelpDeskContext>(new ServiceCollection(),
                CreateDefaultConfiguration(Directory.GetCurrentDirectory(), "appsettings.json", false),
                "DatabaseConfiguration:ConnectionString");
        }

        public bool SaveTestUser(out string status)
        {
            try
            {
                SqlDataContext.Repository<User>().InsertGraph(new User
                {
                    Id = Guid.NewGuid(),
                    Email = "emre.gulay@gmail.com",
                    FullName = "Emre Gulay",
                    IsActive = true,
                    Password = "none"
                });

                SqlDataContext.Save();

                status = "Saved";
                return true;
            }
            catch (Exception ex)
            {
                status = ex.GetAllMessages();
                return false;
            }
        }

        public bool UserExtensionMethodTest(out string status)
        {
            try
            {
                var activeUsers = SqlDataContext.Repository<User>()
                    .GetActiveUsers().ToList();

                if (activeUsers.Any())
                {
                    status = "Success";
                    return true;
                }

                status = "No active user";
                return false;
            }
            catch (Exception ex)
            {
                status = ex.GetAllMessages();
                return false;
            }
        }

        public Request GetRequestById(Guid id)
        {
            try
            {
                return SqlDataContext.Repository<Request>().Find(id);
            }
            catch
            {
                return null;
            }
        }

        public bool SaveRequest(out string status)
        {
            try
            {
                var user = SqlDataContext.Repository<User>().Query()
                    .Get().FirstOrDefault();

                if (user == null)
                {
                    status = "There is no registered User in the database";
                    return false;
                }

                var requestType = SqlDataContext.Repository<RequestType>()
                    .Query().Get().FirstOrDefault();

                if (requestType != null)
                {
                    SqlDataContext.Repository<Request>().InsertGraph(new Request
                    {
                        Id = Guid.NewGuid(),
                        RequestTypeId = requestType.Id,
                        Header = "Test Request",
                        Body = "Body text for test request",
                        UserId = user.Id
                    });

                    SqlDataContext.Save();

                    status = "Success";
                    return true;
                }

                status = "There is no registered Request Type in the database";
                return false;
            }
            catch (Exception ex)
            {
                status = ex.GetAllMessages();
                return false;
            }
        }

        public bool SaveResponse(out string status)
        {
            try
            {
                var user = SqlDataContext.Repository<User>().Query().Get().FirstOrDefault();

                if (user == null)
                {
                    status = "There is no registered User in the database";
                    return false;
                }

                var request = SqlDataContext.Repository<Request>().Query().Include(r => r.Response).Get().FirstOrDefault();
                if (request != null)
                {
                    SqlDataContext.Repository<Response>().InsertGraph(new Response
                    {
                        Id = Guid.NewGuid(),
                        RequestId = request.Id,
                        Header = "Test Response",
                        Body = "Body text for test response",
                        UserId = user.Id
                    });

                    SqlDataContext.Save();

                    status = "Success";
                    return true;
                }

                status = "There is no registered Request in the database";
                return false;
            }
            catch (Exception ex)
            {
                status = ex.GetAllMessages();
                return false;
            }
        }

        public bool SaveBulkResponse(out string status)
        {
            try
            {
                var user = SqlDataContext.Repository<User>().Query().Get().FirstOrDefault();

                if (user == null)
                {
                    status = "There is no registered User in the database";
                    return false;
                }

                var request = SqlDataContext.Repository<Request>().Query().Include(r => r.Response).Get().FirstOrDefault();
                if (request == null)
                {
                    status = "There is no registered Request in the database";
                    return false;
                }

                var responses = Enumerable.Range(0, 1000).Select(p => new Response
                {
                    Id = Guid.NewGuid(),
                    Body = "Bulk insert test for responses",
                    Header = "Bulk insert test",
                    RequestId = request.Id,
                    UserId = user.Id
                });

                SqlDataContext.Repository<Response>().InsertMany(responses);

                SqlDataContext.Save();

                status = "Success";
                return true;
            }
            catch (Exception ex)
            {
                status = ex.GetAllMessages();
                return false;
            }
        }

        public bool DeleteBulkResponse(IEnumerable<Response> responses, out string status)
        {
            try
            {
                SqlDataContext.Repository<Response>().DeleteMany(responses);
                SqlDataContext.Save();
                status = "Success";
                return true;
            }
            catch (Exception ex)
            {
                status = ex.GetAllMessages();
                return false;
            }
        }

        public IQueryable<Response> GetResponses()
        {
            return SqlDataContext.Repository<Response>().Query().Get();
        }

        public IList<Response> ResponseListFromStoredProcedure
                (out string status)
        {
            try
            {
                var request = SqlDataContext.Repository<Request>()
                    .Query().Get().FirstOrDefault();

                if (request != null)
                {
                    var responses = SqlDataContext.Repository<Response>().SqlQuery(
                        "exec [dbo].[spGetResponsesByRequestId] @requestId", new SqlParameter
                        {
                            DbType = DbType.Guid,
                            ParameterName = "@requestId",
                            Direction = ParameterDirection.Input,
                            Value = request.Id
                        }).ToList();

                    if (responses.Any())
                    {
                        status = "Success";
                        return responses;
                    }

                    status = "There is no registered response in the database";
                    return null;
                }

                status = "There is no registered Request in the database";
                return null;
            }
            catch (Exception ex)
            {
                status = ex.GetAllMessages();
                return null;
            }
        }

        public bool WithNoLockQuery(out string status)
        {
            try
            {
                var responses = new List<Response>();
                using (var transaction = SqlDataContext.CreateNewContextTransaction(IsolationLevel.ReadUncommitted))
                {
                    for (var i = 1; i < 150; i++)
                    {
                        responses = SqlDataContext.Repository<Response>().Query()
                            .Include(r => r.User).Get().ToList();
                    }

                    transaction.Commit();
                }

                if (responses.Any())
                {
                    status = "Success";
                    return true;
                }

                status = "There is no registered response in the database";
                return false;
            }
            catch (Exception ex)
            {
                status = ex.GetAllMessages();
                return false;
            }
        }
    }
}
