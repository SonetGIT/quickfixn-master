using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SimpleAcceptor.Models
{
    [Table("accounts")]
    public class account
    {
        public int id { get; set; }
        [Column("account_no")]
        public string accountNo { get; set; }
        [Column("organization_id")]
        public int? organizationId { get; set; }
        [Column("account_type_id")]
        public int? accountTypeId { get; set; }
    }
}
