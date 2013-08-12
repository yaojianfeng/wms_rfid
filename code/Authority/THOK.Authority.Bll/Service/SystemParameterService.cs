using System;
using System.Linq;
using Microsoft.Practices.Unity;
using THOK.Authority.Dal.Interfaces;
using THOK.Authority.Bll.Interfaces;
using THOK.Authority.DbModel;

namespace THOK.Authority.Bll.Service
{
    public class SystemParameterService : ServiceBase<SystemParameter>, ISystemParameterService
    {
        [Dependency]
        public ISystemParameterRepository SystemParameterRepository { get; set; }
        [Dependency]
        public ISystemRepository SystemRepository { get; set; }

        protected override Type LogPrefix
        {
            get { return this.GetType(); }
        }

        public object GetSystemParameter(int page, int rows, string parameterName, string parameterValue, string remark, string userName, string SystemID)
        {
            IQueryable<SystemParameter> systemParameterQuery = SystemParameterRepository.GetQueryable();
            IQueryable<THOK.Authority.DbModel.System> systemQuery = SystemRepository.GetQueryable();

            var systemParameter = systemParameterQuery.Join(systemQuery,
                                         s => s.SystemID,
                                         s1 => s1.SystemID,
                                         (s, s1) => new { s.Id, s.ParameterName, s.ParameterValue, s.Remark, s.UserName, SystemName = s1.SystemName })
                                         .Join(systemQuery,
                                         c => c.SystemName,
                                         s2 => s2.SystemName,
                                         (c, s2) => new { c.Id, c.ParameterName, c.ParameterValue, c.Remark, c.SystemName,c.UserName,SystemID=s2.SystemName })
                                         .Where(p => p.ParameterName.Contains(parameterName) && p.ParameterValue.Contains(parameterValue) && p.Remark.Contains(remark))
                                         .OrderBy(p => p.Id).AsEnumerable()
                                         .Select(p => new
                                         {
                                             Id = p.Id,
                                             p.ParameterName,
                                             p.ParameterValue,
                                             p.Remark,
                                             p.UserName,
                                             p.SystemID,
                                             SystemName = p.SystemID == null ? "" : p.SystemName
                                         });

            int total = systemParameter.Count();
            systemParameter = systemParameter.Skip((page - 1) * rows).Take(rows);
            return new { total, rows = systemParameter.ToArray() };
           
        }

        public bool SetSystemParameter(SystemParameter systemParameter, string userName, out string error)
        {
            error = string.Empty;
            bool result = false;

            var query = SystemParameterRepository.GetQueryable().FirstOrDefault(a => a.Id == systemParameter.Id);
            if (query != null)
            {
                try
                {
                    query.ParameterName = systemParameter.ParameterName;
                    query.ParameterValue = systemParameter.ParameterValue;
                    query.Remark = systemParameter.Remark;
                    query.UserName = userName;
                    query.SystemID = systemParameter.SystemID;

                    SystemParameterRepository.SaveChanges();
                    result = true;
                }
                catch (Exception ex)
                {
                    error = "原因：" + ex.Message;
                }
            }
            else
            {
                error = "原因：未找到当前需要修改的数据！";
            }
            return result;
        }

        public bool AddSystemParameter(SystemParameter systemParameter, string userName, out string error)
        {
            error = string.Empty;
            bool result = false;
            var query = SystemParameterRepository.GetQueryable().FirstOrDefault(a => a.Id == systemParameter.Id);
            if (query == null)
            {
                SystemParameter sp = new SystemParameter();
                if (sp != null)
                {
                    try
                    {
                        sp.ParameterName = systemParameter.ParameterName;
                        sp.ParameterValue = systemParameter.ParameterValue;
                        sp.Remark = systemParameter.Remark;
                        sp.UserName = userName;
                        sp.SystemID = systemParameter.SystemID;

                        SystemParameterRepository.Add(sp);
                        SystemParameterRepository.SaveChanges();
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        error = "原因：" + ex.Message;
                    }
                }
                else
                {
                    error = "原因：找不到当前登陆用户！请重新登陆！";
                }
            }
            else
            {
                error = "原因：该编号已存在！";
            }
            return result;
        }

        public bool DelSystemParameter(int id, out string error)
        {
            error = string.Empty;
            bool result = false;

            var query = SystemParameterRepository.GetQueryable().FirstOrDefault(a => a.Id == id);
            if (query != null)
            {
                try
                {
                    SystemParameterRepository.Delete(query);
                    SystemParameterRepository.SaveChanges();
                    result = true;
                }
                catch (Exception ex)
                {
                    error = "原因：" + ex.Message;
                }
            }
            else
            {
                error = "原因：未找到当前需要删除的数据！";
            }

            return result;
        }

        public bool SetSystemParameter()
        {
            var query = SystemParameterRepository.GetQueryable().FirstOrDefault(a => a.ParameterName == "DownInterFace").ParameterValue;
            if (query == "2")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
