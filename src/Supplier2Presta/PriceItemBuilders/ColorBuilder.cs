using Serilog;
using System;
using System.Collections.Generic;
using Supplier2Presta.Config;

namespace Supplier2Presta.PriceItemBuilders
{
	public class ColorBuilder : IColorBuilder
	{
		private readonly IDictionary<string, string> _colorCodes;

		public ColorBuilder(IRobotConfiguration configuration)
		{
			_colorCodes = new Dictionary<string, string>();
		    foreach (var colorMapping in configuration.ColorMappings)
		    {
		        _colorCodes[colorMapping.Name] = colorMapping.Code;
		    }
		}

		public string GetCode(string colorName)
		{
			colorName = colorName.Replace("ё", "е");

			if (_colorCodes.ContainsKey(colorName))
				return _colorCodes[colorName];

			Log.Warning("Unknown color: {0}", colorName);

			var name = colorName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
			if (_colorCodes.ContainsKey(name))
				return _colorCodes[name];

			if (colorName.IndexOf('/') > 0)
			{
				name = colorName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[0];
				if (_colorCodes.ContainsKey(name))
					return _colorCodes[name];
			}

			return string.Empty;
		}


		public string GetMainColor(string colorName)
		{
			colorName = colorName.Replace("ё", "е");
			if (_colorCodes.ContainsKey(colorName))
			{
				return colorName;
			}

			var name = colorName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
			if (_colorCodes.ContainsKey(name))
			{
				return name;
			}
			else
			{
				if (colorName.IndexOf('/') > 0)
				{
					name = colorName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[0];
					if (_colorCodes.ContainsKey(name))
					{
						return name;
					}
					switch (name)
					{
						case "Серебристый":
							return "Серебро";
						case "Серебристый с черным":
							return "Серебро";
						case "Горячий розовый":
							return "Розовый";
						case "Зеленые мальдивы":
							return "Бирюзовый";
						case "Цветочный":
							return "Белый";
						case "Нежно розовый с черным":
							return "Нежно-розовый";
						case "Металлик красный":
							return "Красный";
						case "Металлик серебро":
							return "Серебро";
						case "Мятный":
							return "Зеленый";
						case "Золото":
							return "Золотой";
						case "Кремовый":
							return "Бежевый";
						case "Сирень":
							return "Сиреневый";
						case "Фуксия":
							return "Сиреневый";
					}
					return name;
				}
				switch (colorName)
				{
					case "Серебристый":
						return "Серебро";
					case "Серебристый с черным":
						return "Серебро";
					case "Горячий розовый":
						return "Розовый";
					case "Зеленые мальдивы":
						return "Бирюзовый";
					case "Цветочный":
						return "Белый";
					case "Нежно розовый с черным":
						return "Нежно-розовый";
					case "Металлик красный":
						return "Красный";
					case "Металлик серебро":
						return "Серебро";
					case "Мятный":
						return "Зеленый";
					case "Золото":
						return "Золотой";
					case "Кремовый":
						return "Бежевый";
					case "Сирень":
						return "Сиреневый";
					case "Фуксия":
						return "Сиреневый";
					case "Зебра с синим":
						return "Зебра";
				}
				return colorName;
			}
		}
	}
}
