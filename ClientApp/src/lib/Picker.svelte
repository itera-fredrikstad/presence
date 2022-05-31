<script lang="ts">
  import {
    useQuery,
    useMutation,
    useQueryClient,
  } from "@sveltestack/svelte-query";
  import {
    addWeeks,
    getMonth,
    format,
    eachDayOfInterval,
    isSaturday,
    isSunday,
    getISOWeek,
    isFirstDayOfMonth,
    formatISO,
  } from "date-fns";
  import { nb } from "date-fns/locale";

  type DayAtTheOffice = {
    userId: string;
    date: string;
    type: "EMPTY" | "FULL" | "FIRST-HALF" | "LAST-HALF";
    comment?: string;
  };

  type DaysAtTheOfficeMap = { [dayId: string]: DayAtTheOffice };

  const queryClient = useQueryClient();

  export let userId: string;

  let today = new Date();
  let offset = 0;

  $: allDays = eachDayOfInterval({
    start: today,
    end: addWeeks(today, offset + 2),
  });
  $: workDays = allDays.filter((day) => !isSaturday(day) && !isSunday(day));
  $: currentWindow = workDays.slice(offset * 5, (offset + 1) * 5);
  $: weeks = Object.keys(
    currentWindow.reduce(
      (prev, next) => ({ ...prev, [getISOWeek(next)]: true }),
      {}
    )
  );
  $: months = currentWindow.reduce<{ [key: string]: Date }>(
    (prev, next) => ({ ...prev, [getMonth(next)]: next }),
    {}
  );

  function handleNext() {
    offset++;
  }

  function handlePrev() {
    if (offset > 0) {
      offset--;
    }
  }

  function getKey(date: Date) {
    return format(date, "ddMMyy");
  }

  function isSelected(store: any, date: Date) {
    if (!store) {
      return false;
    }

    const day = store[getKey(date)];

    return !!day && day.type !== "EMPTY";
  }

  function save(obj) {
    $mutation.mutate(obj);
  }

  function handleSelect(date: Date) {
    const dayId = getKey(date);

    if ($query.data?.[dayId] && $query.data?.[dayId].type === "FULL") {
      save({
        userId,
        date: formatISO(date),
        type: "FIRST-HALF",
      });
      return;
    }

    if ($query.data?.[dayId] && $query.data?.[dayId].type === "FIRST-HALF") {
      save({
        userId,
        date: formatISO(date),
        type: "LAST-HALF",
      });
      return;
    }

    if ($query.data?.[dayId] && $query.data?.[dayId].type === "LAST-HALF") {
      save({
        userId,
        date: formatISO(date),
        type: "EMPTY",
      });
      return;
    }

    save({
      userId,
      date: formatISO(date),
      type: "FULL",
    });
  }

  async function postData(url = "", data = {}) {
    // Default options are marked with *
    const response = await fetch(url, {
      method: "PUT", // *GET, POST, PUT, DELETE, etc.
      mode: "cors", // no-cors, *cors, same-origin
      cache: "no-cache", // *default, no-cache, reload, force-cache, only-if-cached
      credentials: "same-origin", // include, *same-origin, omit
      headers: {
        "Content-Type": "application/json",
        // 'Content-Type': 'application/x-www-form-urlencoded',
      },
      redirect: "follow", // manual, *follow, error
      referrerPolicy: "no-referrer", // no-referrer, *no-referrer-when-downgrade, origin, origin-when-cross-origin, same-origin, strict-origin, strict-origin-when-cross-origin, unsafe-url
      body: JSON.stringify(data), // body data type must match "Content-Type" header
    });

    return response.bodyUsed ? response.json() : {}; // parses JSON response into native JavaScript objects
  }

  const query = useQuery(["dayAtWorks", userId], () =>
    fetch("https://localhost:7080/dayAtWork")
      .then((r) => r.json())
      .then((j) =>
        (j as DayAtTheOffice[]).reduce(
          (p, n) => ({ ...p, [getKey(new Date(n.date))]: n }),
          {}
        )
      )
  );

  const mutation = useMutation(
    (newDayAtWork: DayAtTheOffice) =>
      postData("https://localhost:7080/dayAtWork", newDayAtWork),
    {
      // When mutate is called:
      onMutate: async (newTodo) => {
        // Cancel any outgoing refetches (so they don't overwrite our optimistic update)
        await queryClient.cancelQueries(["dayAtWorks", newTodo.userId]);

        // Snapshot the previous value
        const previousTodos = queryClient.getQueryData([
          "dayAtWorks",
          newTodo.userId,
        ]);

        // Optimistically update to the new value
        queryClient.setQueryData(["dayAtWorks", newTodo.userId], (old: {}) => ({
          ...old,
          [getKey(new Date(newTodo.date))]: newTodo,
        }));

        // Return a context object with the snapshotted value
        return { previousTodos };
      },
      // If the mutation fails, use the context returned from onMutate to roll back
      onError: (err, newTodo, context: any) => {
        queryClient.setQueryData(
          ["dayAtWorks", newTodo.userId],
          context.previousTodos
        );
      },
      // Always refetch after error or success:
      onSettled: () => {
        queryClient.invalidateQueries(["dayAtWorks"]);
      },
    }
  );
</script>

<div class="picker-root">
  <h1>Registrering</h1>
  <h2>
    Uke {weeks.join("/")} ({Object.values(months)
      .map((d) => format(d, "MMMM", { locale: nb }))
      .join("/")})
  </h2>
  <div class="wrapper">
    {#if offset > 0}
      <i class="fa-solid fa-circle-arrow-left" on:click={handlePrev} />
    {/if}
    <i class="fa-solid fa-circle-arrow-right" on:click={handleNext} />
    <div class="window">
      <div
        class="days slide"
        style="transform: translateX({offset * -100}%); transform-origin: 0 0"
      >
        {#each workDays as day}
          {@const dayId = getKey(day)}
          <div
            class:selected={isSelected($query.data, day)}
            class:first-half={isSelected($query.data, day) &&
              $query.data?.[dayId].type === "FIRST-HALF"}
            class:last-half={isSelected($query.data, day) &&
              $query.data?.[dayId].type === "LAST-HALF"}
            class="day"
            on:click={(e) => handleSelect(day)}
            on:contextmenu|preventDefault={(e) => alert(e)}
          >
            <h1>
              {format(day, "d", { locale: nb })}
              {#if isFirstDayOfMonth(day)}{format(day, "MMMM", {
                  locale: nb,
                })}{/if}
            </h1>
            <h2>{format(day, "eeee", { locale: nb })}</h2>
            <!--<textarea 
                                readonly={true} 
                                class="inactive"
                                on:click="{e => {
                                    e.stopImmediatePropagation();
                                    e.preventDefault();
                                }}"/>-->
          </div>
        {/each}
      </div>
    </div>
  </div>
</div>

<style>
  .picker-root {
    width: 100%;
  }

  h1 {
    font-size: 2rem;
  }

  h2 {
    font-size: 1.5rem;
  }

  .wrapper {
    position: relative;
    width: 100%;
  }

  .window {
    overflow: hidden;
    width: 100%;
  }

  .days {
    display: flex;
  }

  .slide {
    transition: all 0.5s;
  }

  .day {
    aspect-ratio: 1;
    background-color: #efefef;
    padding: 1rem;
    border: 1px solid #999;
    display: flex;
    flex-direction: column;
    align-content: flex-start;
    justify-content: flex-start;
    align-items: flex-start;
    flex-basis: 20%;
    flex-shrink: 0;
  }

  .day:hover {
    background-color: #eaeaea;
  }

  .day textarea {
    border: none;
    background: #efefef;
    padding: 0.2rem 0.5rem;
    width: 100%;
    margin: 0;
    font-size: 0.8rem;
    resize: none;
    overflow: hidden;
  }

  .day textarea:focus {
    outline: none;
  }

  .day textarea.inactive {
    user-select: none;
  }

  .day.selected {
    background: repeating-linear-gradient(
      -45deg,
      #efefef,
      #efefef 5px,
      #ffcccb 5px,
      #ffcccb 10px
    );
  }

  .day.selected.first-half {
    background: linear-gradient(
        to top,
        rgba(238, 238, 238, 1) 0%,
        rgba(238, 238, 238, 1) 50%,
        rgba(238, 238, 238, 0) 50%,
        rgba(238, 238, 238, 0) 100%
      ),
      repeating-linear-gradient(
        -45deg,
        #efefef,
        #efefef 5px,
        #ffcccb 5px,
        #ffcccb 10px
      );
  }

  .day.selected.last-half {
    background: linear-gradient(
        to bottom,
        rgba(238, 238, 238, 1) 0%,
        rgba(238, 238, 238, 1) 50%,
        rgba(238, 238, 238, 0) 50%,
        rgba(238, 238, 238, 0) 100%
      ),
      repeating-linear-gradient(
        -45deg,
        #efefef,
        #efefef 5px,
        #ffcccb 5px,
        #ffcccb 10px
      );
  }

  .day:last-child {
    border: none;
  }

  .day h1 {
    margin: 0;
    font-size: 1rem;
    background-color: #efefef;
    padding: 0.2rem 0.5rem;
  }

  .day h2 {
    margin: 0;
    font-size: 1.2rem;
    background-color: #efefef;
    padding: 0.2rem 0.5rem;
  }

  .fa-solid {
    color: #999;
  }

  .fa-solid:hover {
    color: #333;
  }

  .fa-circle-arrow-right {
    position: absolute;
    right: -100px;
    top: 50%;
    transform: translateY(-50%);
    font-size: 2rem;
  }

  .fa-circle-arrow-left {
    position: absolute;
    left: -100px;
    top: 50%;
    transform: translateY(-50%);
    font-size: 2rem;
  }
</style>
