using Crimson.Core.Common;

namespace Assets.Crimson.Core.Common.Filters
{
	public interface IFilter<T>
	{
		bool Filter(T target);
	}
}