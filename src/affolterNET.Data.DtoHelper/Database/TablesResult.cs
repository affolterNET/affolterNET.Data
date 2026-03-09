using System;
using System.Collections.Generic;

namespace affolterNET.Data.DtoHelper.Database
{
    public class TablesResult
    {
        public TablesResult(IEnumerable<string>? schemas = null)
        {
            if (schemas != null)
            {
                Schemas.AddRange(schemas);
            }
        }

        public string? Error { get; set; }

        public Exception? Ex { get; set; }

        public List<string> Schemas { get; } = new List<string>();

        public Tables Tables { get; set; } = new Tables();
    }
}