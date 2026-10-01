using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Article
    {
        public int Id { get; set; }
        public bool IsVisible { get; set; }
        public string Name { get; set; }
    }
}