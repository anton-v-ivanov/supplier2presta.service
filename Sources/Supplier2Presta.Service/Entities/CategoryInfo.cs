namespace Supplier2Presta.Service.Entities
{
	public class CategoryInfo
	{
		public string Name { get; }
		public string SubName { get; }

		public CategoryInfo(string name, string subName)
		{
			Name = name;
			SubName = subName;
		}
	}
}