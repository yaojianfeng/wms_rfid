using THOK.Authority.DbModel;

namespace THOK.Authority.Bll.Interfaces
{
    public interface ISystemParameterService : IService<SystemParameter>
    {
        bool SetSystemParameter(SystemParameter systemParameter, string userName, out string error);
        bool AddSystemParameter(SystemParameter systemParameter, string userName, out string error);
        bool DelSystemParameter(int id, out string error);
        bool SetSystemParameter();

        object GetSystemParameter(int page, int rows, string parameterName, string parameterValue, string remark, string userName, string SystemID);
    }
}
