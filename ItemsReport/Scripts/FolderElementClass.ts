//
// class FolderElementClass
//

class FolderElementClass
{
    private openItemChar : string = "\u25e2";
    private closeItemChar : string = "\u25b7";
    
    private readonly _markSpanElement : HTMLSpanElement;

    public constructor(markSpanElement : HTMLSpanElement)
    {
        this._markSpanElement = markSpanElement;
    }

    public get IsClose()
    {
        return this._markSpanElement.innerText === this.closeItemChar;
    }

    public Open()
    {
        this._markSpanElement.innerText = this.openItemChar;
    }

    public Close()
    {
        this._markSpanElement.innerText = this.closeItemChar;
    }

    public static GetElementByOwnerId(src: HTMLElement | null | string): (FolderElementClass | null)    
    {
        if (src === null)
        {
            return null;
        }

        let custedElement = (src as string) ? document.getElementById(<string>src) : <HTMLElement>(src);

        let spanList: HTMLCollectionOf<HTMLSpanElement> = custedElement.getElementsByTagName("span");
        for (let i = 0; i < spanList.length; i++)
        {
            const element: HTMLElement = spanList[i];
            if (element.id === "mark")
            {
                return new FolderElementClass(element);
            }
        }

        return null;
    }

    public static CloseAllChilds(ownerId: string)
    {
        let childList: HTMLCollectionOf<Element> = document.getElementsByClassName(`childOf_${ownerId}`);

        if (childList.length === 0)
        {
            return;
        }

        for (let i = 0; i < childList.length; i++)
        {
            let child: HTMLElement = <HTMLElement>(childList[i]);
            FolderElementClass.CloseAllChilds(child.id);

            child.style.display = "none";
        }

        let folderElement = FolderElementClass.GetElementByOwnerId(ownerId);
        folderElement?.Close();
    }
}
