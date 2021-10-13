using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SimpleAcceptor.Models
{
    [Table("finance_instruments")]
    public class financeInstrument
    {
        public int id { get; set; }
        public string code { get; set; }
    }
}
