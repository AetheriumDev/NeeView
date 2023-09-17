﻿using NeeView.Native;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{


    class WicDecoders
    {

        /// <summary>
        /// Collect WIC Decoders
        /// </summary>
        /// <returns>friendlyName to fileExtensions dictionary</returns>
        public static Dictionary<string, string> ListUp()
        {
            var collection = new Dictionary<string, string>();

            try
            {
                var friendlyName = new StringBuilder(2048);
                var fileExtensions = new StringBuilder(2048);
                for (uint i = 0; NVInterop.NVGetImageCodecInfo(i, friendlyName, fileExtensions); ++i)
                {
                    ////Debug.WriteLine($"{friendlyName}: {fileExtensions}");
                    var key = friendlyName.ToString();
                    if (collection.ContainsKey(key))
                    {
                        collection[key] = collection[key].TrimEnd(',') + ',' + fileExtensions.ToString().ToLower();
                    }
                    else
                    {
                        collection.Add(key, fileExtensions.ToString().ToLower());
                    }
                }
                NVInterop.NVCloseImageCodecInfo();
            }
            finally
            {
                NVInterop.NVFpReset();
            }

            return collection;
        }
    }
}
