﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DSLNG.PEAR.Data.Enums;

namespace DSLNG.PEAR.Data.Entities
{
    public class KpiTarget : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Kpi Kpi { get; set; }
        //public PmsConfigDetails PmsConfigDetail { get; set; }
        public double? Value { get; set; }
        public DateTime Periode { get; set; }
        public PeriodeType PeriodeType { get; set; }
        public string Remark { get; set; }

        public bool IsActive { get; set; }
        
    }

}
