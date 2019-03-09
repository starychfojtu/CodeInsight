namespace CodeInsight.Library
{
    public abstract class NewType<A>
    {
        protected NewType(A value)
        {
            Value = value;
        }
        
        public A Value { get; }

        protected bool Equals(NewType<A> other) =>
            Value.Equals(other.Value);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NewType<A>) obj);
        }

        public override int GetHashCode() =>
            Value.GetHashCode();

        public static implicit operator A(NewType<A> n) =>
            n.Value;

        public override string ToString() =>
            Value.ToString();
    }
}