﻿using THOK.Authority.DbModel;

namespace THOK.Authority.Bll.Interfaces
{
    public interface IHelpContentService : IService<HelpContent>
    {
        bool Add(HelpContent helpContent, out string strResult);
    }
}
