<script lang="ts">
  import {
    useQuery,
    useMutation,
    useQueryClient,
  } from "@sveltestack/svelte-query";
  import { format } from "date-fns";
  import {
    addOrUpdateDayAtWork,
    getDayAtWorkItemsForUser,
    getPublicHolidays,
    getTeamEvents,
  } from "../api";

  import type { DayAtWork, Identifiable } from "../models";
  import { getDayId } from "../utils";

  import MonthLayout from "./MonthLayout.svelte";
  import WorkWeekLayout from "./WorkWeekLayout.svelte";

  import Toggle from "svelte-toggle";

  const queryClient = useQueryClient();

  export let userId: string;

  let displayMonthView: boolean = false;
  let today = new Date();

  $: currentYear = format(today, "yyyy");

  $: holidayQuery = useQuery(
    ["publicHolidays", currentYear],
    () => getPublicHolidays(currentYear),
    {
      staleTime: Infinity,
      cacheTime: Infinity,
    }
  );

  $: query = useQuery("dayAtWorks", () => getDayAtWorkItemsForUser(), {
    staleTime: 1000 * 60 * 10, // 10 minutes
    cacheTime: 1000 * 60 * 10, // 10 minutes
  });

  const teamEventsQuery = useQuery("teamEvents", () => getTeamEvents(), {
    staleTime: 1000 * 60 * 10, // 10 minutes,
    cacheTime: 1000 * 60 * 10, // 10 minutes
  });

  const mutation = useMutation(
    (newDayAtWork: Identifiable<DayAtWork>) =>
      addOrUpdateDayAtWork(newDayAtWork),
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
  <div class="heading-container">
    <h1>Registrering</h1>
    <div class="toggle-container">
      <Toggle
        bind:toggled={displayMonthView}
        label="Månedsvisning"
        toggledColor="#ff4b33"
      />
    </div>
  </div>
  {#if displayMonthView}
    <MonthLayout
      {today}
      dayAtWorks={$query.data}
      publicHolidays={$holidayQuery.data}
      teamEvents={$teamEventsQuery.data}
      onUpdate={handleUpdate}
    />
  {:else}
    <WorkWeekLayout
      {today}
      dayAtWorks={$query.data}
      publicHolidays={$holidayQuery.data}
      teamEvents={$teamEventsQuery.data}
      onUpdate={handleUpdate}
    />
  {/if}
</div>

<style>
  .picker-root {
    width: 100%;
  }

  .heading-container {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }
</style>
