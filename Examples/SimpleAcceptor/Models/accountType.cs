using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SimpleAcceptor.Models
{
    [Table("account_types")]
    public class accountType
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
