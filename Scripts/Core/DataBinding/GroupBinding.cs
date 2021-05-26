using System;

[Serializable]
public class GroupBinding {
    public event Action<string> OnMemberChanged;

    protected void Notify(string memberName){
        OnMemberChanged?.Invoke(memberName);
    }
}