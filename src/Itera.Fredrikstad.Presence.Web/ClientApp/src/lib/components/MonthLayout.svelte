<script lang="ts">
  import {
    format,
    eachDayOfInterval,
    startOfISOWeek,
    endOfISOWeek,
    startOfMonth,
    endOfMonth,
    addMonths,
    isSameDay,
    getWeeksInMonth,
    getWeekOfMonth,
  } from "date-fns";
  import { nb } from "date-fns/locale";

  import type {
    DayAtWorkItemsMap,
    PublicHolidayMap,
    TeamEventMap,
  } from "../api";
  import type { DayAtWork, Identifiable } from "../models";
  import { getDayId } from "../utils";

  import Day from "./Day.svelte";

  export let today: Date;
  export let dayAtWorks: DayAtWorkItemsMap;
  export let publicHolidays: PublicHolidayMap;
  export let teamEvents: TeamEventMap;
  export let onUpdate: (
    day: Date,
    dayAtWork: Identifiable<DayAtWork>,
    updatedDayAtWork: Partial<DayAtWork>
  ) => void;

  let offset = getWeeksInMonth(today) - getWeekOfMonth(today) < 1 ? 1 : 0;

  let currentMonthStart = startOfMonth(today);
  let currentMonthEnd = endOfMonth(today);

  $: windows = [...Array(offset + 2).keys()].map((o) =>
    eachDayOfInterval({
      start: startOfISOWeek(startOfMonth(addMonths(currentMonthStart, o))),
      end: endOfISOWeek(endOfMonth(addMonths(currentMonthEnd, o))),
    })
  );

  $: currentYear = format(windows[offset][0], "yyyy");
  $: currentMonth = format(addMonths(currentMonthStart, offset), "MMMM", {
    locale: nb,
  });

  function handleNext() {
    offset++;
  }

  function handlePrev() {
    if (offset > 0) {
      offset--;
    }
  }
</script>

<div class="selector-wrapper">
  <h2>{currentMonth} {currentYear}</h2>
  <div>
    <i class="fa-solid fa-circle-arrow-left" on:click={handlePrev} />
    <i class="fa-solid fa-circle-arrow-right" on:click={handleNext} />
  </div>
</div>
<div class="wrapper">  
  <div class="slide-container">
    <div class="days headers">
      <div class="day-wrapper">mandag</div>
      <div class="day-wrapper">tirsdag</div>
      <div class="day-wrapper">onsdag</div>
      <div class="day-wrapper">torsdag</div>
      <div class="day-wrapper">fredag</div>
      <div class="day-wrapper">lørdag</div>
      <div class="day-wrapper">søndag</div>
    </div>
  </div>
  <div class="slide-container">
    <div
      class="slider slide"
      style="transform: translateX({offset * -100}%); transform-origin: 0 0"
    >
      {#each windows as window}
        <div class="window-container">
          <div class="days">
            {#each window as day}
              {@const dayId = getDayId(day)}
              {@const dayAtWork = dayAtWorks?.[dayId]}
              {@const publicHoliday = publicHolidays?.[dayId]}
              {@const events = teamEvents?.[dayId] ?? []}
              <div class="day-wrapper">
                <Day
                  {day}
                  {dayAtWork}
                  {publicHoliday}
                  teamEvents={events}
                  showDayName={false}
                  isActive={day > today || isSameDay(day, today)}
                  onUpdate={(updated) => onUpdate(day, dayAtWork, updated)}
                />
              </div>
            {/each}
          </div>
        </div>
      {/each}
    </div>
  </div>
</div>

<style>
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

  .slide-container {
    width: 100%;
    overflow: hidden;
  }

  .slider {
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

  .days.headers .day-wrapper {
    text-align: center;
    padding: 1rem;
  }

  .day-wrapper {
    flex-basis: calc(100% / 7);
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
    font-size: 1.5rem;
  }

  .fa-circle-arrow-left {
    font-size: 1.5rem;
  }
</style>
