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
    startOfISOWeek,
    endOfISOWeek,
    formatISO,
    startOfMonth,
    endOfMonth,
    addMonths,
  } from "date-fns";
  import { nb } from "date-fns/locale";
  import {
    addOrUpdateDayAtWork,
    getDayAtWorkItemsForUser,
    getPublicHolidays,
    getTeamEvents,
  } from "../api";
  import Day from "./Day.svelte";

  import type { DayAtWork, Identifiable } from "../models";
  import { getDayId } from "../utils";

  const queryClient = useQueryClient();

  export let userId: string;

  let today = new Date();
  let offset = 0;

  let currentMonthStart = startOfMonth(today);
  let currentMonthEnd = endOfMonth(today);

  $: windows = [...Array(offset + 2).keys()].map((o) =>
    eachDayOfInterval({
      start: startOfISOWeek(startOfMonth(addMonths(currentMonthStart, o))),
      end: endOfISOWeek(endOfMonth(addMonths(currentMonthEnd, o))),
    })
  );

  $: console.log([...Array(offset + 1).keys()]);

  $: holidayQuery = useQuery(["publicHolidays", "2022"], () =>
    getPublicHolidays("2022")
  );
  $: query = useQuery(["dayAtWorks"], () => getDayAtWorkItemsForUser());

  const teamEventsQuery = useQuery("teamEvents", () => getTeamEvents());

  function handleNext() {
    offset++;
  }

  function handlePrev() {
    if (offset > 0) {
      offset--;
    }
  }

  const mutation = useMutation(
    (newDayAtWork: Identifiable<DayAtWork>) =>
      addOrUpdateDayAtWork(newDayAtWork),
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

    $mutation.mutate(updatedDaw);
  }
</script>

<div class="picker-root">
  <h1>Registrering</h1>
  <div class="selector-wrapper">
    <h2 />
  </div>
  <div class="wrapper">
    {#if offset > 0}
      <i class="fa-solid fa-circle-arrow-left" on:click={handlePrev} />
    {/if}
    <i class="fa-solid fa-circle-arrow-right" on:click={handleNext} />
    <div class="foo-wrapper slide">
      <div
        class="foo-bar-wrapper slide"
        style="transform: translateX({offset * -100}%); transform-origin: 0 0"
      >
        {#each windows as window}
          <div class="window-container">
            <div class="days">
              {#each window.slice(0, 7) as weekDay}
                {@const dayName = format(weekDay, "eeee", { locale: nb })}
                <div class="day-wrapper">
                  {dayName}
                </div>
              {/each}
              {#each window as day}
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
                    showDayName={false}
                    onUpdate={(updated) =>
                      handleUpdate(day, dayAtWork, updated)}
                  />
                </div>
              {/each}
            </div>
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

  .selector-wrapper {
    display: flex;
    align-items: center;
    justify-content: space-between;
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

  .foo-wrapper {
    width: 100%;
    overflow: hidden;
  }

  .foo-bar-wrapper {
    display: flex;
  }

  .window-container {
    overflow: hidden;
    width: 100%;
    flex-shrink: 0;
  }

  .days {
    display: grid;
    grid-template-columns: 1fr 1fr 1fr 1fr 1fr 1fr 1fr;
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
