<script lang="ts">
  import { useQuery, useMutation, useQueryClient } from "@sveltestack/svelte-query";
  import { addWeeks, getMonth, format, eachDayOfInterval, getISOWeek } from "date-fns";
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
  let numDays = 5;

  $: allDays = eachDayOfInterval({
    start: today,
    end: addWeeks(today, offset + 2),
  });
  $: workDays = allDays;
  $: currentWindow = workDays.slice(offset * numDays, (offset + 1) * numDays);
  $: weeks = Object.keys(
    currentWindow.reduce((prev, next) => ({ ...prev, [getISOWeek(next)]: true }), {})
  );
  $: months = currentWindow.reduce<{ [key: string]: Date }>(
    (prev, next) => ({ ...prev, [getMonth(next)]: next }),
    {}
  );
  $: currentYear = format(currentWindow[0], "yyyy");

  $: holidayQuery = useQuery(["publicHolidays", currentYear], () => getPublicHolidays(currentYear), {
    staleTime: Infinity,
    cacheTime: Infinity
  });
  
  $: query = useQuery("dayAtWorks", () => getDayAtWorkItemsForUser(), {
    staleTime: 1000 * 60 * 10, // 10 minutes
    cacheTime: 1000 * 60 * 10 // 10 minutes
  });

  const teamEventsQuery = useQuery("teamEvents", () => getTeamEvents(), {
    staleTime: 1000 * 60 * 10, // 10 minutes,
    cacheTime: 1000 * 60 * 10
  });

  function handleNext() {
    offset++;
  }

  function handlePrev() {
    if (offset > 0) {
      offset--;
    }
  }

  const mutation = useMutation(
    (newDayAtWork: Identifiable<DayAtWork>) => addOrUpdateDayAtWork(newDayAtWork),
    {
      // When mutate is called:
      onMutate: async (newDayAtWork) => {
        // Cancel any outgoing refetches (so they don't overwrite our optimistic update)
        await queryClient.cancelQueries("dayAtWorks");

        // Snapshot the previous value
        const previousDayAtWorks = queryClient.getQueryData("dayAtWorks");

        // Optimistically update to the new value
        queryClient.setQueryData("dayAtWorks", (old: {}) => ({
          ...old,
          [newDayAtWork.id]: newDayAtWork,
        }));

        // Return a context object with the snapshotted value
        return { previousDayAtWorks };
      },
      onSuccess: (data) => {
        queryClient.setQueryData("dayAtWorks", (old: {}) => ({
          ...old,
          [getDayId(data.date)]: data,
        }));
      },
      // If the mutation fails, use the context returned from onMutate to roll back
      onError: (err, newTodo, context: any) => {
        queryClient.setQueryData("dayAtWorks", context.previousDayAtWorks);
      },
      // Always refetch after error or success:
      onSettled: (data, error, variables, context) => {
        queryClient.invalidateQueries(["daySummary", variables.date]);
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
    <h2>
      Uke {weeks.join("/")} ({Object.values(months)
        .map((d) => format(d, "MMMM", { locale: nb }))
        .join("/")})
    </h2>
  </div>
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
          {@debug day}
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
              isActive={true}
              onUpdate={(updated) => handleUpdate(day, dayAtWork, updated)}
            />
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

  .window {
    overflow: hidden;
    width: 100%;
  }

  .days {
    display: flex;
  }

  @media only screen and (max-width: 480px) {
    .days {
      flex-direction: column;
    }
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

  :global(.dark) .fa-solid:hover {
    color: #fff;
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
