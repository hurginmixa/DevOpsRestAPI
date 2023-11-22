﻿//
// class MarkSpanObject
//

class MarkSpanObject
{
    private openItem : string = "\u25e2";
    private closeItem : string = "\u25b7";
    
    private readonly _markSpanElement : HTMLSpanElement;

    public constructor(markSpanElement : HTMLSpanElement)
    {
        this._markSpanElement = markSpanElement;
    }

    public get IsClose()
    {
        return this._markSpanElement.innerText === this.closeItem;
    }

    public Open()
    {
        this._markSpanElement.innerText = this.openItem;
    }

    public Close()
    {
        this._markSpanElement.innerText = this.closeItem;
    }

    public static GetMarkSpanElement(src: HTMLElement | null): (MarkSpanObject | null)    
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
                return new MarkSpanObject(element);
            }
        }

        return null;
    }
}
