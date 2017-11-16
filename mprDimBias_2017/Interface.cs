using System;
using System.Collections.Generic;
using ModPlusAPI.Interfaces;

namespace mprDimBias
{
    public class Interface : IModPlusFunctionInterface
    {
        public SupportedProduct SupportedProduct => SupportedProduct.Revit;
        public string Name => "mprDimBias";
        public string AvailProductExternalVersion => "2017";
        public string FullClassName => "mprDimBias.Application.MprDimBiasCommand";
        public string AppFullClassName => "mprDimBias.Application.MprDimBiasApp";
        public Guid AddInId => Guid.Parse("424d234f-a1b8-4a33-a702-c83589650435");
        public string LName => "Смещение размеров";
        public string Description => "Автоматическое перемещение размерного значения, попадающего на размерные линии";
        public string Author => "Пекшев Александр aka Modis";
        public string Price => "0";
        public bool CanAddToRibbon => false;
        public string FullDescription => "Функция следит за всеми создающимися размерами и, в случае если размерный текст попадает на размерные линии, смещает размерный текст в сторону";
        public string ToolTipHelpImage => "";
        public List<string> SubFunctionsNames => new List<string>();
        public List<string> SubFunctionsLames => new List<string>();
        public List<string> SubDescriptions => new List<string>();
        public List<string> SubFullDescriptions => new List<string>();
        public List<string> SubHelpImages => new List<string>();
        public List<string> SubClassNames => new List<string>();
    }
}
