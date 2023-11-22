//
// class LineMarkerClass
//

class LineMarkerClass
{
    private readonly savedRowElement: HTMLTableRowElement;
    private readonly savedBackgroundColor: string;
    private readonly savedTextColor: string;
    private readonly savedAnchorColor: string;

    public constructor(rowElement: HTMLTableRowElement)
    {
        this.savedRowElement = rowElement;
        this.savedBackgroundColor =  this.savedRowElement.style.backgroundColor;
        this.savedTextColor = this.savedRowElement.style.color;

        this.savedRowElement.style.backgroundColor = "#707b7c";
        this.savedRowElement.style.color = "#fdfefe";

        let anchorList : HTMLCollectionOf<HTMLAnchorElement> = this.savedRowElement.getElementsByTagName("a");
        if (anchorList.length > 0)
        {
            this.savedAnchorColor = anchorList[0].style.color;

            for (let i = 0; i < anchorList.length; i++)
            {
                anchorList[i].style.color = "#fdfefe";
            }
        }
    }

    public get ItemId() : number
    {
        return +this.savedRowElement.id;
    }

    public Hide() : void
    {
        this.savedRowElement.style.backgroundColor = this.savedBackgroundColor;
        this.savedRowElement.style.color = this.savedTextColor;

        let anchorList : HTMLCollectionOf<HTMLAnchorElement> = this.savedRowElement.getElementsByTagName("a");
        if (anchorList.length > 0)
        {
            for (let i = 0; i < anchorList.length; i++)
            {
                anchorList[i].style.color = this.savedAnchorColor;
            }
        }
    }
}
