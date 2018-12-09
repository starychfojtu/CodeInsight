namespace CodeInsight.Library
{
    public abstract class NewType<A>
    {
        private readonly A value;

        protected NewType(A value)
        {
            this.value = value;
        }

        protected bool Equals(NewType<A> other) =>
            value.Equals(other.value);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NewType<A>) obj);
        }

        public override int GetHashCode() =>
            value.GetHashCode();

        public static implicit operator A(NewType<A> n) =>
            n.value;

        public override string ToString() =>
            value.ToString();
    }
}