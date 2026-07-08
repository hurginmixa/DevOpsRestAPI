/// <reference path="FolderElementClass.ts" />
/// <reference path="AzureTools.ts" />

// Текущий набор колонок, который сервер впечатал в страницу.
declare var reportedPaths: string[];

// Переключение светлой/тёмной темы (data-theme на <html>).
function toggleTheme(): void
{
    const root = document.documentElement;
    root.setAttribute("data-theme", root.getAttribute("data-theme") === "dark" ? "" : "dark");
}

// Фильтр из тулбара: приглушаем строки, не совпавшие с запросом.
function onFilter(query: string): void
{
    const q = query.trim().toLowerCase();
    const rows = document.getElementsByClassName("item");
    for (let i = 0; i < rows.length; i++)
    {
        const row = rows[i] as HTMLElement;
        const hit = !q || row.innerText.toLowerCase().indexOf(q) >= 0;
        row.style.opacity = hit ? "" : ".28";
    }
}

// Всплывающее уведомление вместо alert().
function toast(message: string, isError: boolean = false): void
{
    const box = document.getElementById("toasts");
    if (!box)
    {
        return;
    }

    const el = document.createElement("div");
    el.className = isError ? "toast err" : "toast";
    el.innerHTML = `<span class="tdot"></span>${message}`;
    box.appendChild(el);

    setTimeout(() =>
    {
        el.style.transition = "opacity .3s";
        el.style.opacity = "0";
        setTimeout(() => el.remove(), 320);
    }, 2600);
}

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
    // (по цепочке childOf_). Корень ищем заново каждый раз, чтобы после замены
    // собрать уже новые строки, а не удалённые старые.
    function collectSubtreeRows(): HTMLTableRowElement[]
    {
        const rows: HTMLTableRowElement[] = [];

        const root = document.getElementById(`${id}`) as HTMLTableRowElement | null;
        if (root)
        {
            rows.push(root);
        }

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

    const icon = rootRow.querySelector(".refresh") as HTMLElement | null;
    icon?.classList.add("spinning");

    try
    {
        const response = await fetch(`refresh/${id}/rows`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ columns: reportedPaths, color: rootRow.style.getPropertyValue("--rail") })
        });

        if (!response.ok)
        {
            toast(`Не удалось обновить item ${id}: ${response.status}`, true);
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

        // Подсвечиваем свежие строки (старые уже удалены → соберём заново).
        for (const row of collectSubtreeRows())
        {
            row.classList.remove("flash");
            void row.offsetWidth;
            row.classList.add("flash");
        }

        toast(`Item ${id} обновлён с Azure`);
    }
    catch (error)
    {
        toast(`Ошибка обновления item ${id}: ${error}`, true);
    }
    finally
    {
        icon?.classList.remove("spinning");
    }
}

function onDocumentClick(ev: Event)
{
    if (!(ev.target instanceof HTMLTableCellElement)) 
    {
        return;
    }

    const rowElement: HTMLTableRowElement = ev.target.parentElement as HTMLTableRowElement;
    if (!rowElement || +rowElement.id <= 0)
    {
        return;
    }

    // Пометить/снять: помеченной может быть только одна строка.
    const wasMarked = rowElement.classList.contains("marked");

    const prev = document.querySelector("tr.marked");
    if (prev)
    {
        prev.classList.remove("marked");
    }

    if (!wasMarked)
    {
        rowElement.classList.add("marked");
    }

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
