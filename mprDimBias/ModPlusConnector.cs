#pragma warning disable SA1600 // Elements should be documented
namespace mprDimBias
{
    using System;
    using System.Collections.Generic;
    using ModPlusAPI.Interfaces;

    public class ModPlusConnector : IModPlusFunctionInterface
    {
        public SupportedProduct SupportedProduct => SupportedProduct.Revit;
        
        public string Name => "mprDimBias";
        
#if R2016
        public string AvailProductExternalVersion => "2016";
#elif R2017
        public string AvailProductExternalVersion => "2017";
#elif R2018
        public string AvailProductExternalVersion => "2018";
#elif R2019
        public string AvailProductExternalVersion => "2019";
#elif R2020
        public string AvailProductExternalVersion => "2020";
#elif R2021
        public string AvailProductExternalVersion => "2021";
#endif
        
        public string FullClassName => "mprDimBias.Application.MprDimBiasCommand";
        
        public string AppFullClassName => "mprDimBias.Application.MprDimBiasApp";
        
        public Guid AddInId => Guid.Parse("424d234f-a1b8-4a33-a702-c83589650435");
        
        public string LName => "Смещение размеров";
        
        public string Description => "Автоматическое перемещение размерного значения, попадающего на размерные линии";
        
        public string Author => "Пекшев Александр aka Modis";
        
        public string Price => "0";
        
        public bool CanAddToRibbon => false;
        
        public string FullDescription => "Плагин следит за всеми создающимися размерами и, в случае если размерный текст попадает на размерные линии, смещает размерный текст в сторону";
        
        public string ToolTipHelpImage => string.Empty;
        
        public List<string> SubFunctionsNames => new List<string>();
        
        public List<string> SubFunctionsLames => new List<string>();
        
        public List<string> SubDescriptions => new List<string>();
        
        public List<string> SubFullDescriptions => new List<string>();
        
        public List<string> SubHelpImages => new List<string>();
        
        public List<string> SubClassNames => new List<string>();
    }
}
#pragma warning restore SA1600 // Elements should be documented