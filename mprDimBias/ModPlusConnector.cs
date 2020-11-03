namespace mprDimBias
{
    using System;
    using System.Collections.Generic;
    using ModPlusAPI.Abstractions;
    using ModPlusAPI.Enums;

    /// <inheritdoc/>
    public class ModPlusConnector : IModPlusPlugin
    {
        /// <inheritdoc/>
        public SupportedProduct SupportedProduct => SupportedProduct.Revit;

        /// <inheritdoc/>
        public string Name => "mprDimBias";

#if R2017
        /// <inheritdoc/>
        public string AvailProductExternalVersion => "2017";
#elif R2018
        /// <inheritdoc/>
        public string AvailProductExternalVersion => "2018";
#elif R2019
        /// <inheritdoc/>
        public string AvailProductExternalVersion => "2019";
#elif R2020
        /// <inheritdoc/>
        public string AvailProductExternalVersion => "2020";
#elif R2021
        /// <inheritdoc/>
        public string AvailProductExternalVersion => "2021";
#endif

        /// <inheritdoc/>
        public string FullClassName => "mprDimBias.Application.MprDimBiasCommand";

        /// <inheritdoc/>
        public string AppFullClassName => "mprDimBias.Application.MprDimBiasApp";

        /// <inheritdoc/>
        public Guid AddInId => Guid.Parse("424d234f-a1b8-4a33-a702-c83589650435");

        /// <inheritdoc/>
        public string LName => "Смещение размеров";

        /// <inheritdoc/>
        public string Description => "Автоматическое перемещение размерного значения, попадающего на размерные линии";

        /// <inheritdoc/>
        public string Author => "Пекшев Александр aka Modis";

        /// <inheritdoc/>
        public string Price => "0";

        /// <inheritdoc/>
        public bool CanAddToRibbon => false;

        /// <inheritdoc/>
        public string FullDescription => "Плагин следит за всеми создающимися размерами и, в случае если размерный текст попадает на размерные линии, смещает размерный текст в сторону";

        /// <inheritdoc/>
        public string ToolTipHelpImage => string.Empty;

        /// <inheritdoc/>
        public List<string> SubPluginsNames => new List<string>();

        /// <inheritdoc/>
        public List<string> SubPluginsLNames => new List<string>();

        /// <inheritdoc/>
        public List<string> SubDescriptions => new List<string>();

        /// <inheritdoc/>
        public List<string> SubFullDescriptions => new List<string>();

        /// <inheritdoc/>
        public List<string> SubHelpImages => new List<string>();

        /// <inheritdoc/>
        public List<string> SubClassNames => new List<string>();
    }
}