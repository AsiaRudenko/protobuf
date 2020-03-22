#nullable enable

namespace Zenserdes.Protobuf
{
	/// <summary>
	/// A helper interface, which marks a given message as both a message and an <see cref="IMessageAndOperator{TSelf}"/>.
	/// This plug in easily to the static methods provided in <see cref="ZenserdesProtobuf"/>,
	/// and ease the amount of generics the user has to explicitly provide.
	/// </summary>
	/// <typeparam name="TSelf">Itself.</typeparam>
	public interface IMessageAndOperator<TSelf> : IMessage, IMessageOperator<TSelf>
		where TSelf : struct, IMessageAndOperator<TSelf>
	{
	}
}