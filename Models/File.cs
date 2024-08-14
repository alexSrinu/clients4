using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Clients.Models
{
    public class File
    {
        [Required]
        public string FileName { get; set; }
        [Required]
        public string Base64FileData { get; set; }
        public string FileUploadLocation { get; set; }
        public string FileURLLocation { get; set; }
        public string _ext { get; set; }
    }
}