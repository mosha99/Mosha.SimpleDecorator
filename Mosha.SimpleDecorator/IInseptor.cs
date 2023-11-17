public interface IInseptor
{
    public object target { get; set; }
    public void SetModel(object obj) => target = obj;
    public object Insept(Invoker invoker);
}