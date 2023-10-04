export class MarkDiv
{
    private openItem = "\u25e2";
    private closeItem = "\u25b7";
    
    private _markSpanElement : HTMLSpanElement;

    constructor(markSpanElement : HTMLSpanElement)
    {
        this._markSpanElement = markSpanElement;
    }

    get IsClose()
    {
        return this._markSpanElement.innerText === this.closeItem;
    }

    Open()
    {
        this._markSpanElement.innerText = this.openItem;
    }

    Close()
    {
        this._markSpanElement.innerText = this.closeItem;
    }

    static GetMarkSpanElement(src: HTMLElement | null): (MarkDiv | null)    
    {
        if (src === null)
        {
            return null;
        }

        let spanList: HTMLCollectionOf<HTMLSpanElement> = src.getElementsByTagName("span");
        for (let i = 0; i < spanList.length; i++)
        {
            const element: HTMLElement = spanList[i];
            if (element.id === "mark")
            {
                return new MarkDiv(element);
            }
        }

        return null;
    }

}
