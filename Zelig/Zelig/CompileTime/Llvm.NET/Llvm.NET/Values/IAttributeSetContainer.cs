namespace Llvm.NET.Values
{
    /// <summary>Interface for values containing an AttributeSet</summary>
    /// <remarks>
    /// This is used to allow the <see cref="AttributeSetContainer"/> extension
    /// to act as mutators for the otherwise immutable <see cref="AttributeSet"/>.
    /// Each method of the extension class will read the attribute set from the container
    /// and create a new set based on the parameters (adding or removing attributes from the set)
    /// producing a new attributeSet that is then re-assigned back to the container. 
    /// </remarks>
    public interface IAttributeSetContainer
    {
        /// <summary>Attributes for this container</summary>
        AttributeSet Attributes { get; set; }
    }
}
