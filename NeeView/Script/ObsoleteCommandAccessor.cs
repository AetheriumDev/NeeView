﻿using System;
using System.Collections.Generic;

namespace NeeView
{
    public class ObsoleteCommandAccessor : ICommandAccessor 
    {
        private string _name;
        private IAccessDiagnostics _accessDiagnostics;
        private ObsoleteCommandItem _obsoleteInfo;

        public ObsoleteCommandAccessor(string commandName, ObsoleteCommandItem obsoleteInfo, IAccessDiagnostics accessDiagnostics)
        {
            _name = commandName;
            _accessDiagnostics = accessDiagnostics;
            _obsoleteInfo = obsoleteInfo;
        }


        public bool IsShowMessage
        {
            get => false;
            set { }
        }
        
        public string MouseGesture
        {
            get => "";
            set { }
        }

        public string ShortCutKey
        {
            get => "";
            set { }
        }
        
        public string TouchGesture
        {
            get => "";
            set { }
        }

        public PropertyMap? Parameter => null;


        public bool Execute(params object[] args)
        {
            return false;
        }

        public CommandAccessor? Patch(IDictionary<string, object> patch)
        {
            return null;
        }


        internal ObsoleteAttribute GetObsoleteAttribute()
        {
            return new ObsoleteAttribute();
        }

        internal AlternativeAttribute? GetAlternativeAttribute()
        {
            return _obsoleteInfo != null ? new AlternativeAttribute(_obsoleteInfo.Alternative, _obsoleteInfo.Version) : null;
        }

        internal string? CreateObsoleteCommandMessage()
        {
            return RefrectionTools.CreateObsoleteMessage(_name, GetObsoleteAttribute(), GetAlternativeAttribute());
        }
    }
}
