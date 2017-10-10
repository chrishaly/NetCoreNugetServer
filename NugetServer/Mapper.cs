namespace NugetServer
{
	public static class Mapper
	{
		static Mapper()
		{
			AutoMapper.Mapper.Initialize(config => { config.CreateMissingTypeMaps = true; });
		}

		public static TDestination Map<TSource, TDestination>(TSource source)
		{
			return AutoMapper.Mapper.Map<TDestination>(source);
		}

		public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
		{
			return AutoMapper.Mapper.Map(source, destination);
		}
	}
}