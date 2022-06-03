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
    getISOWeek,
    formatISO,
  } from "date-fns";
  import { nb } from "date-fns/locale";
  import Select from "svelte-select";
  import { getDayAtWorkItemsForUser, getPublicHolidays, getTeamEvents } from "../api";
  import Day from "./Day.svelte";
  import type { DayAtWork, Identifiable } from "../models";
  import { users } from "../users";
  import { getDayId } from "../utils";

  const queryClient = useQueryClient();

  let userId: string = null;

  let today = new Date();
  let offset = 0;
  let numDays = 5;

  $: allDays = eachDayOfInterval({
    start: today,
    end: addWeeks(today, offset + 2),
  });
  $: workDays = allDays;
  $: currentWindow = workDays.slice(offset * numDays, (offset + 1) * numDays);
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
  $: currentYear = format(currentWindow[0], "yyyy");

  $: holidayQuery = useQuery(["publicHolidays", currentYear], () =>
    getPublicHolidays(currentYear)
  );
  $: query =
    userId &&
    useQuery(["dayAtWorks", userId], () => getDayAtWorkItemsForUser(userId));

  const teamEventsQuery = useQuery("teamEvents", () => getTeamEvents());

  function handleNext() {
    offset++;
  }

  function handlePrev() {
    if (offset > 0) {
      offset--;
    }
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

  const mutation = useMutation(
    (newDayAtWork: any) => postData("api/dayAtWork", newDayAtWork),
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
          [newTodo.id]: newTodo,
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

  function createDayAtWork(date: Date): Identifiable<DayAtWork> {
    return {
      id: getDayId(date),
      userId,
      date,
      type: "FULL",
    };
  }

  function handleUpdate(
    day: Date,
    dayAtWork: Identifiable<DayAtWork>,
    updatedDayAtWork: Partial<DayAtWork>
  ) {
    const daw = dayAtWork ?? createDayAtWork(day);
    const updatedDaw: Identifiable<DayAtWork> = { ...daw, ...updatedDayAtWork };

    $mutation.mutate({
      ...updatedDaw,
      date: formatISO(updatedDaw.date, { representation: "date" }),
    });
  }

  function handleSelectUser(event: any) {
    userId = event?.detail?.value;
  }
</script>

<div class="picker-root">
  <h1>Registrering</h1>
  <div class="selector-wrapper">
    <h2>
      Uke {weeks.join("/")} ({Object.values(months)
        .map((d) => format(d, "MMMM", { locale: nb }))
        .join("/")})
    </h2>
    <div class="select-container">
      <Select 
        items={Object.keys(users).map(u => ({ value: u, label: users[u] }))} 
        placeholder="Velg bruker" 
        showChevron={true}
        on:select="{handleSelectUser}" />
    </div>
  </div>
  {#if !userId}
    <h3>Velg en bruker i nedtrekkslisten.</h3>
  {:else}
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
            {@const dayId = getDayId(day)}
            {@const dayAtWork = $query?.data?.[dayId]}
            {@const publicHoliday = $holidayQuery?.data?.[dayId]}
            {@const teamEvents = $teamEventsQuery?.data?.[dayId] ?? []}
            <div class="day-wrapper">
              <Day
                {day}
                {dayAtWork}
                {publicHoliday}
                {teamEvents}
                onUpdate={(updated) => handleUpdate(day, dayAtWork, updated)}
              />
            </div>
          {/each}
        </div>
      </div>
    </div>
  {/if}
</div>

<style>
  .picker-root {
    width: 100%;
  }

  .selector-wrapper {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  .select-container {
    --border: none;
    width: 30%;
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

  .day-wrapper {
    flex-basis: calc(100% / 5);
    flex-shrink: 0;
    flex-direction: column;
  }

  .slide {
    transition: all 0.5s;
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
