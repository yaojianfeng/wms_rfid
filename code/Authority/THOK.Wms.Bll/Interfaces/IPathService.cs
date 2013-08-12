using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using THOK.Wms.DbModel;

namespace THOK.Wms.Bll.Interfaces
{
    public interface IPathService : IService<Path>
    {
        object GetDetails(int page, int rows, Path path);

        bool Add(Path path);

        bool Save(Path path, out string strResult);

        bool Delete(int pathId);

        object GetPath(int page, int rows, string queryString, string value);

        System.Data.DataTable GetPath(int page, int rows, Path path);

        //bool Save(Path path, string strResult);
    }
}
