/// <reference path="FolderElementClass.ts" />
/// <reference path="LineMarkerClass.ts" />
/// <reference path="AzureTools.ts" />

let LineMarker: LineMarkerClass | null = null;

// Текущий набор колонок, который сервер впечатал в страницу.
declare var reportedPaths: string[];

// Клик по иконке ↻ у item'а 1-го уровня: просим сервер перечитать поддерево
// и отдать готовые <tr> (POST /refresh/{id}/rows), затем заменяем ими старые.
async function OnRefreshClick(id: number): Promise<void>
{
    const rootRow = document.getElementById(`${id}`) as HTMLTableRowElement | null;
    if (!rootRow)
    {
        return;
    }

    // Локальная функция — нужна только здесь. Строка корня + все её потомки
    // (по цепочке childOf_).
    function collectSubtreeRows(): HTMLTableRowElement[]
    {
        const rows: HTMLTableRowElement[] = [rootRow];

        const walk = (ownerId: number) =>
        {
            const children = document.getElementsByClassName(`childOf_${ownerId}`);
            for (let i = 0; i < children.length; i++)
            {
                const child = children[i] as HTMLTableRowElement;
                rows.push(child);
                walk(+child.id);
            }
        };

        walk(id);

        return rows;
    }

    try
    {
        const response = await fetch(`refresh/${id}/rows`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ columns: reportedPaths, color: rootRow.style.backgroundColor })
        });

        if (!response.ok)
        {
            alert(`Refresh failed: ${response.status}`);
            return;
        }

        const html = await response.text();

        // Вставляем новые строки перед корнем, затем удаляем старое поддерево.
        const oldRows = collectSubtreeRows();
        rootRow.insertAdjacentHTML("beforebegin", html);
        for (const row of oldRows)
        {
            row.remove();
        }
    }
    catch (error)
    {
        alert(`Refresh error: ${error}`);
    }
}

function onDocumentClick(ev: Event)
{
    if (!(ev.target instanceof HTMLTableCellElement)) 
    {
        return;
    }

    const cellElement: HTMLTableCellElement = ev.target as HTMLTableCellElement;
    const rowElement: HTMLTableRowElement = cellElement.parentElement as HTMLTableRowElement;

    let currentId : number = +rowElement.id;
    if (currentId <= 0)
    {
        return;
    }

    let oldIdNumber : number = (LineMarker?.ItemId ?? -1);
    LineMarker?.Hide();

    LineMarker = oldIdNumber === currentId ? null : new LineMarkerClass(rowElement);

    ev.cancelBubble = true;
}

function OnCollapseAll()
{
    let childList: HTMLCollectionOf<Element> = document.getElementsByClassName(`childOf_0`);
    for (let i = 0; i < childList.length; i++)
    {
        const element: HTMLElement = <HTMLElement>(childList[i]);
        FolderElementClass.CloseAllChilds(element.id);
    }
}

function OnMarkClick(markSpanElement: HTMLElement, ownerId: string) : boolean
{
    if (!(markSpanElement instanceof HTMLSpanElement))
    {
        return true;
    }

    let markSpan: FolderElementClass = new FolderElementClass(markSpanElement);

    if (markSpan.IsClose)
    {
        markSpan.Open();

        let subItemList: HTMLCollectionOf<Element> = document.getElementsByClassName(`childOf_${ownerId}`);
        for (let i = 0; i < subItemList.length; i++)
        {
            const element: HTMLElement = <HTMLElement>(subItemList[i]);
            element.style.display = "";
        }

        return false;
    }

    if (!markSpan.IsClose)
    {
        FolderElementClass.CloseAllChilds(ownerId);

        return false;
    }

    return true;
}
