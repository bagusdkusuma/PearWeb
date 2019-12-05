﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSLNG.PEAR.Web.ViewModels.Menu
{
    public class MenuViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public IList<Menu> Menus { get; set; }
        public int Order { get; set; }
        public bool IsRoot { get; set; }
        public ICollection<RoleGroup> RoleGroups { get; set; }
        public string Remark { get; set; }
        public string Module { get; set; }

        public bool IsActive { get; set; }

        public class Menu
        {
            public int Id { get; set; }

            public string Name { get; set; }
            public IList<Menu> Menus { get; set; }
            public int Order { get; set; }
            public bool IsRoot { get; set; }
            public ICollection<RoleGroup> RoleGroups { get; set; }
            public string Remark { get; set; }
            public string Module { get; set; }

            public bool IsActive { get; set; }
        }


        public class RoleGroup
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Level Level { get; set; }
            public string Icon { get; set; }
            public string Remark { get; set; }
            public bool IsActive { get; set; }
        }


        public class Level
        {
            public int Id { get; set; }

            public string Code { get; set; }
            public string Name { get; set; }
            public int Number { get; set; }
            public string Remark { get; set; }

            public bool IsActive { get; set; }
        }
    }
}